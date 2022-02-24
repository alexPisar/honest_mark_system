using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigSet.Configs;
using System.Security.Cryptography.X509Certificates;
using Diadoc.Api.Proto.Events;
using Diadoc.Api.Proto.Docflow;
using Diadoc.Api.Proto.Documents;
using Diadoc.Api.Proto;
using Diadoc.Api;

namespace WebSystems.WebClients
{
    public class DiadocEdoClient : WebClientSingleInstance<DiadocEdoClient>
    {
        private readonly Config _config = Config.GetInstance();
        private DiadocHttpApi _api;
        private X509Certificate2 _certificate;
        private DiadocEdoTokenCache _cache;
        private static readonly object syncRoot = new object();
        private string _authToken => _cache.Token ?? "";
        private string _actualBoxId => _cache.ActualBoxId ?? "";

        /// <summary>
		/// Истёк ли токен аутентификации
		/// </summary>
		public bool IsTokenExpired => _cache?.TokenExpirationDate < DateTime.Now;

        private DiadocEdoClient() : base()
        {
            var hClient = new Diadoc.Api.Http.HttpClient(Properties.Settings.Default.DiadocApiUrl);
            // если edo хочет ходить через прокси - пусть будет так
            if (_config.ProxyEnabled)
            {
                hClient.SetProxyUri("http://" + _config.ProxyAddress);
                hClient.UseSystemProxy = false;
                hClient.SetProxyCredentials(new System.Net.NetworkCredential(
                    _config.ProxyUserName,
                    _config.ProxyUserPassword
                ));
            }

            _api = new DiadocHttpApi(_config.DiadocApiId, hClient, new Diadoc.Api.Cryptography.WinApiCrypt());
        }

        public new static DiadocEdoClient GetInstance()
        {
            if (_instance == null)
                _instance = new DiadocEdoClient();

            return _instance;
        }

        public Message SendXmlDocument(string senderOrgId,
            string recipientOrgId,
            bool isOurRecipient,
            DocumentAttachment documentAttachment = null)
        {
            OrganizationList myOrganizations = CallApiSafe(new Func<OrganizationList>(() => _api.GetMyOrganizations(_authToken, false)));

            if ((myOrganizations?.Organizations?.Count ?? 0) == 0)
                throw new Exception("Не найдены свои организации по токену.");

            var senderOrganization = myOrganizations.Organizations.FirstOrDefault(o => o.OrgId == senderOrgId);

            if (senderOrganization == null)
                throw new Exception($"Не найдена организация с ID отправителя {senderOrgId}");

            Organization recipientOrganization;

            if (isOurRecipient)
            {
                recipientOrganization = myOrganizations.Organizations.FirstOrDefault(o => o.OrgId == recipientOrgId);

                if (recipientOrganization == null)
                    throw new Exception($"Не найдена своя организация-отправитель с ID {recipientOrgId}");
            }
            else
            {
                var counteragents = GetKontragents(senderOrgId);

                counteragents = counteragents.Where(c => c.Organization?.OrgId == recipientOrgId)?
                    .ToList() ?? new List<Counteragent>();

                if (counteragents.Count == 0)
                    throw new Exception("Не найдены контрагенты");

                var counteragent = counteragents.First();

                if (counteragent?.Organization == null)
                    throw new Exception("Не найдена организация");

                recipientOrganization = counteragent.Organization;
            }

            var messageToPost = new MessageToPost
            {
                FromBoxId = senderOrganization.Boxes.First().BoxId,
                ToBoxId = recipientOrganization.Boxes.First().BoxId
            };

            messageToPost.DocumentAttachments.Add(documentAttachment);
            return CallApiSafe(new Func<Message>(() => { return _api.PostMessage(_authToken, messageToPost); }));
        }

        public MessagePatch SendPatchRecipientXmlDocument(string messageId, int docType, RecipientTitleAttachment recipientAttachment)
        {
            var messageToPost = new MessagePatchToPost
            {
                BoxId = _actualBoxId,
                MessageId = messageId
            };

            if(docType == (int)DocumentType.UniversalTransferDocument)
                messageToPost.AddUniversalTransferDocumentBuyerTitle(recipientAttachment);
            else if(docType == (int)DocumentType.XmlTorg12)
                messageToPost.AddXmlTorg12BuyerTitle(recipientAttachment);
            else if (docType == (int)DocumentType.XmlAcceptanceCertificate)
                messageToPost.AddXmlAcceptanceCertificateBuyerTitle(recipientAttachment);

            return CallApiSafe(new Func<MessagePatch>(() => { return _api.PostMessagePatch(_authToken, messageToPost); }));
        }

        public List<Counteragent> GetKontragents(string orgId = null)
        {
            CounteragentList list;

            if (string.IsNullOrEmpty(orgId))
            {
                OrganizationList MyOrganizations = CallApiSafe(new Func<OrganizationList>(() => _api.GetMyOrganizations(_authToken, false)));
                Organization myOrganization = MyOrganizations.Organizations.First();

                list = CallApiSafe(new Func<CounteragentList>(() => { return _api.GetCounteragents(_authToken, myOrganization.OrgId, "IsMyCounteragent", null); }));
            }
            else
            {
                list = CallApiSafe(new Func<CounteragentList>(() => { return _api.GetCounteragents(_authToken, orgId, "IsMyCounteragent", null); }));
            }
            return list.Counteragents;
        }

        public Organization GetMyOrganizationByInnKpp(string inn, string kpp = null)
        {
            OrganizationList myOrganizations = CallApiSafe(new Func<OrganizationList>(() => _api.GetMyOrganizations(_authToken, false)));

            Organization organization;

            if (!string.IsNullOrEmpty(kpp))
                organization = myOrganizations.Organizations.FirstOrDefault(o => o.Inn == inn && o.Kpp == kpp);
            else
                organization = myOrganizations.Organizations.FirstOrDefault(o => o.Inn == inn);

            return organization;
        }

        public List<DocflowEvent> GetEvents(DateTime fromDate, DateTime? toDate = null)
        {
            var request = new GetDocflowEventsRequest
            {
                Filter = new TimeBasedFilter
                {
                    FromTimestamp = new Timestamp(fromDate.Ticks)
                },
                AfterIndexKey = null
            };

            if (toDate != null)
                request.Filter.ToTimestamp = new Timestamp(toDate.Value.Ticks);

            var eventsResponse = CallApiSafe(new Func<GetDocflowEventsResponse>(() => _api.GetDocflowEvents(_authToken, _actualBoxId, request)));
            var events = eventsResponse.Events;

            return events;
        }

        public List<DocumentWithDocflow> GetEvents(string messageId, string entityId, bool includedContent = false)
        {
            var requests = new GetDocflowBatchRequest
            {
                Requests =
                {
                    new GetDocflowRequest
                    {
                        DocumentId = new DocumentId(messageId, entityId),
                        InjectEntityContent = includedContent
                    }
                }
            };

            var eventsResponse = CallApiSafe(new Func<GetDocflowBatchResponse>(() => _api.GetDocflows(_authToken, _actualBoxId, requests)));
            var events = eventsResponse.Documents;

            return events;
        }

        public Document GetDocument(string messageId, string entityId)
        {
            var document = CallApiSafe(new Func<Document>(() => _api.GetDocument(_authToken, _actualBoxId, messageId, entityId)));
            return document;
        }

        public void SendReceipt(string messageId, string entityId)
        {
            var cryptoUtil = new UtilitesLibrary.Service.CryptoUtil(_certificate);

            var firstMiddleName = cryptoUtil.ParseCertAttribute(_certificate.Subject, "G");
            string signerFirstName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
            string signerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;

            var receipt = _api.GenerateReceiptXml(_authToken, _actualBoxId, messageId, entityId, 
                new Diadoc.Api.Proto.Invoicing.Signer
                {
                    SignerCertificate = _certificate.RawData,
                    SignerCertificateThumbprint = _certificate.Thumbprint,
                    SignerDetails = new Diadoc.Api.Proto.Invoicing.SignerDetails()
                    {
                        Surname = cryptoUtil.ParseCertAttribute(_certificate.Subject, "SN"),
                        FirstName = signerFirstName,
                        Patronymic = signerPatronymic,
                        Inn = cryptoUtil.ParseCertAttribute(_certificate.Subject, "ИНН").TrimStart('0'),
                        JobTitle = cryptoUtil.ParseCertAttribute(_certificate.Subject, "T")
                    }
                });

            var receiptAttachment = new ReceiptAttachment
            {
                ParentEntityId = entityId,
                SignedContent = new SignedContent
                {
                    Content = receipt.Content,
                    Signature = cryptoUtil.Sign(receipt.Content, true)
                }
            };

            var postMessage = new MessagePatchToPost
            {
                BoxId = _actualBoxId,
                MessageId = messageId
            };

            postMessage.AddReceipt(receiptAttachment);

            var messagePatch = CallApiSafe(new Func<MessagePatch>(() => _api.PostMessagePatch(_authToken, postMessage)));
        }

        public void SendInvoiceCorrectionDocument(string messageId, string entityId, byte[] fileBytes)
        {
            var cryptoUtil = new UtilitesLibrary.Service.CryptoUtil(_certificate);

            var firstMiddleName = cryptoUtil.ParseCertAttribute(_certificate.Subject, "G");
            string signerFirstName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
            string signerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;
            

            var invoiceCorrectionAttachment = new CorrectionRequestAttachment()
            {
                ParentEntityId = entityId,
                SignedContent = new SignedContent
                {
                    Content = fileBytes,
                    Signature = cryptoUtil.Sign(fileBytes, true)
                }
            };

            var postMessage = new MessagePatchToPost
            {
                BoxId = _actualBoxId,
                MessageId = messageId
            };

            postMessage.AddCorrectionRequest(invoiceCorrectionAttachment);

            var messagePatch = CallApiSafe(new Func<MessagePatch>(() => _api.PostMessagePatch(_authToken, postMessage)));
        }

        public void SendRejectionDocument(string messageId, string entityId, byte[] fileBytes)
        {
            var cryptoUtil = new UtilitesLibrary.Service.CryptoUtil(_certificate);

            var firstMiddleName = cryptoUtil.ParseCertAttribute(_certificate.Subject, "G");
            string signerFirstName = firstMiddleName.IndexOf(" ") > 0 ? firstMiddleName.Substring(0, firstMiddleName.IndexOf(" ")) : string.Empty;
            string signerPatronymic = firstMiddleName.IndexOf(" ") >= 0 && firstMiddleName.Length > firstMiddleName.IndexOf(" ") + 1 ? firstMiddleName.Substring(firstMiddleName.IndexOf(" ") + 1) : string.Empty;


            var signatureRejectionAttachment = new XmlSignatureRejectionAttachment()
            {
                ParentEntityId = entityId,
                SignedContent = new SignedContent
                {
                    Content = fileBytes,
                    Signature = cryptoUtil.Sign(fileBytes, true)
                }
            };

            var postMessage = new MessagePatchToPost
            {
                BoxId = _actualBoxId,
                MessageId = messageId
            };

            postMessage.AddXmlSignatureRejectionAttachment(signatureRejectionAttachment);

            var messagePatch = CallApiSafe(new Func<MessagePatch>(() => _api.PostMessagePatch(_authToken, postMessage)));
        }

        public List<Document> GetDocuments(string filterCategory, DateTime dateFrom, DateTime? dateTo = null)
        {
            var documentsFilter = new DocumentsFilter
            {
                BoxId = _actualBoxId,
                FilterCategory = filterCategory,
                TimestampFrom = dateFrom,
                TimestampTo = dateTo
            };

            var documentsList = CallApiSafe(new Func<DocumentList>(() => _api.GetDocuments(_authToken, documentsFilter)));
            return documentsList.Documents;
        }

        public List<Document> GetDocumentsByMessageId(string messageId)
        {
            var documentsList = CallApiSafe(new Func<DocumentList>(() => _api.GetDocumentsByMessageId(_authToken, _actualBoxId, messageId)));
            return documentsList.Documents;
        }

        public Message GetMessage(string messageId, string entityId)
        {
            var message = CallApiSafe(new Func<Message>(() => _api.GetMessage(_authToken, _actualBoxId, messageId, entityId)));
            return message;
        }

        /// <summary>
		/// Получить токен аутентификации
		/// </summary>
		public bool Authenticate(X509Certificate2 cert = null)
        {
            if (cert != null)
                _certificate = cert;
            else
                _certificate = new X509Certificate2(System.IO.File.ReadAllBytes(_config.CertFullPath));

            _cache = new DiadocEdoTokenCache().Load(_certificate.Thumbprint);

            if (_cache != null && !IsTokenExpired)
            {
                return true;
            }
            if (_cache == null || IsTokenExpired)
            {
                string authToken;

                authToken = (string)CallApiSafe(new Func<object>(() => _api.Authenticate(_certificate.RawData)));

                if (string.IsNullOrEmpty(authToken))
                    return false;

                var certInn = new UtilitesLibrary.Service.CryptoUtil().GetOrgInnFromCertificate(_certificate);

                _cache = new DiadocEdoTokenCache(authToken, $"Certificate, Serial Number {_certificate.SerialNumber}", _cache?.PartyId ?? "");

                var myOrg = GetMyOrganizationByInnKpp(certInn);

                if (myOrg == null)
                    throw new Exception("Не найдена организация, соответствующая самому сертификату подписанта.");

                _cache.ActualBoxId = myOrg.Boxes.First().BoxId;
                _cache.Save(_cache, _certificate.Thumbprint);

                return true;
            }
            return false;
        }

        private TOut CallApiSafe<TOut>(Func<TOut> CallingDelegate) where TOut : new()
        {
            TOut ret;
            int tries = 15;
            Exception ex = null;

            while (tries-- >= 0)
            {
                try
                {
                    ret = CallingDelegate.Invoke();
                    return ret;
                }
                catch (System.Net.WebException webEx)
                {
                    ex = webEx;
                }
                catch (Exception e)
                {
                    ex = e;
                }
            }

            if (ex != null)
                throw ex;

            throw new Exception("метод не получилось вызвать более 15 раз");
        }
    }
}
