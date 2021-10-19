using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using Cryptography.WinApi;
using ConfigSet.Configs;

namespace WebSystems.WebClients
{
    public class EdoLiteClient : WebClientSingleInstance<EdoLiteClient>
    {
        private X509Certificate2 _certificate;
        private string _token;
        private ServiceManager _webService;

        private EdoLiteClient() : base()
        {
            if (Config.GetInstance().ProxyEnabled)
                _webService = new ServiceManager(Config.GetInstance().ProxyAddress,
                    Config.GetInstance().ProxyUserName,
                    Config.GetInstance().ProxyUserPassword);
            else
                _webService = new ServiceManager();
        }

        public new static EdoLiteClient GetInstance()
        {
            if (_instance == null)
                _instance = new EdoLiteClient();

            return _instance;
        }

        public bool Authorization(X509Certificate2 certificate)
        {
            var cache = new EdoLiteTokenCache().Load(certificate.Thumbprint);

            if (cache?.Token != null && cache?.TokenExpirationDate > DateTime.Now)
            {
                _token = cache.Token;
                _certificate = certificate;
                return true;
            }

            var authData = _webService.GetRequest<Models.AuthRequest>($"{Properties.Settings.Default.UrlAddressEdoLite}/api/v1/session");

            var crypto = new WinApiCryptWrapper(certificate);

            byte[] signedData = Encoding.UTF8.GetBytes(authData.Data);
            var signature = crypto.Sign(signedData, false);

            var authRequest = new Models.AuthRequest
            {
                Uid = authData.Uid,
                Data = Convert.ToBase64String(signature)
            };

            var authRequestJson = JsonConvert.SerializeObject(authRequest);

            var result = _webService.PostRequest<Models.AuthResult>($"{Properties.Settings.Default.UrlAddressEdoLite}/api/v1/session",
                authRequestJson, null, "application/json");

            if (result.ErrorMessage != null)
                throw new Exception($"Произошла ошибка с кодом {result.ErrorCode}:{result.ErrorMessage} /nОписание:{result.Description}");

            if (string.IsNullOrEmpty(result.Token))
                throw new Exception("Не удалось получить токен авторизации.");

            _token = result.Token;

            cache = new EdoLiteTokenCache()
            {
                Token = result.Token,
                TokenCreationDate = DateTime.Now,
                TokenExpirationDate = DateTime.Now.AddHours(12)
            };
            cache.Save(cache, certificate.Thumbprint);
            _certificate = certificate;

            return true;
        }

        public Models.EdoLiteDocumentList GetIncomingDocumentList(int limit = 10, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var authData = new Dictionary<string, string>();
            authData.Add("Authorization", $"Bearer {_token}");

            string url = $"{Properties.Settings.Default.UrlAddressEdoLite}/api/v1/incoming-documents?limit={limit}";

            if(dateFrom != null)
            {
                var timestamp = new UtilitesLibrary.Service.DateTimeUtil().GetTimestampByDateTime(dateFrom.Value);
                url = $"{url}&&created_from={timestamp}";
            }

            if (dateTo != null)
            {
                var timestamp = new UtilitesLibrary.Service.DateTimeUtil().GetTimestampByDateTime(dateTo.Value);
                url = $"{url}&&created_to={timestamp}";
            }

            var documentList = _webService.GetRequest<Models.EdoLiteDocumentList>(url, authData);
            return documentList;
        }

        public Models.EdoLiteDocumentList GetOutgoingDocumentList(int limit = 10, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var authData = new Dictionary<string, string>();
            authData.Add("Authorization", $"Bearer {_token}");

            string url = $"{Properties.Settings.Default.UrlAddressEdoLite}/api/v1/outgoing-documents?limit={limit}";

            if (dateFrom != null)
            {
                var timestamp = new UtilitesLibrary.Service.DateTimeUtil().GetTimestampByDateTime(dateFrom.Value);
                url = $"{url}&&created_from={timestamp}";
            }

            if (dateTo != null)
            {
                var timestamp = new UtilitesLibrary.Service.DateTimeUtil().GetTimestampByDateTime(dateTo.Value);
                url = $"{url}&&created_to={timestamp}";
            }

            var documentList = _webService.GetRequest<Models.EdoLiteDocumentList>(url, authData);
            return documentList;
        }

        public byte[] GetIncomingDocumentContent(string documentId)
        {
            var authData = new Dictionary<string, string>();
            authData.Add("Authorization", $"Bearer {_token}");

            var docContentStr = _webService.GetRequest($"{Properties.Settings.Default.UrlAddressEdoLite}/api/v1/incoming-documents/{documentId}/content", authData, Encoding.GetEncoding(1251));

            var fileBytes = Encoding.GetEncoding(1251).GetBytes(docContentStr);
            return fileBytes;
        }

        public byte[] GetOutgoingDocumentContent(string documentId)
        {
            var authData = new Dictionary<string, string>();
            authData.Add("Authorization", $"Bearer {_token}");

            var docContentStr = _webService.GetRequest($"{Properties.Settings.Default.UrlAddressEdoLite}/api/v1/outgoing-documents/{documentId}/content", authData, Encoding.GetEncoding(1251));

            var fileBytes = Encoding.GetEncoding(1251).GetBytes(docContentStr);
            return fileBytes;
        }
    }
}
