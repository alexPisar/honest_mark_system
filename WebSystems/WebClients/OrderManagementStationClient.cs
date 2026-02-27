using System;
using System.Security.Cryptography.X509Certificates;
using ConfigSet.Configs;
using Cryptography.WinApi;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace WebSystems.WebClients
{
    public class OrderManagementStationClient : WebClientSingleInstance<OrderManagementStationClient>
    {
        private ServiceManager _webService;
        private X509Certificate2 _certificate;
        private string _token;
        private string _omsId;

        private OrderManagementStationClient() : base()
        {
            if (Config.GetInstance().ProxyEnabled)
                _webService = new ServiceManager(Config.GetInstance().ProxyAddress,
                    Config.GetInstance().ProxyUserName,
                    Config.GetInstance().GetProxyPassword());
            else
                _webService = new ServiceManager();
        }

        public new static OrderManagementStationClient GetInstance()
        {
            if (_instance == null)
                _instance = new OrderManagementStationClient();

            return _instance;
        }

        public Models.OMS.OrderBlocks GetBlockIdsFromOrder(string orderId, string gtin)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var result = _webService.GetRequest<Models.OMS.OrderBlocks>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/order/codes/blocks?omsId={_omsId}&orderId={orderId}&gtin={gtin}",
                headerData, "application/json");

            return result;
        }

        public Models.OMS.MarkedCodesFromReport GetMarkedCodesFromReport(string reportId)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var result = _webService.GetRequest<Models.OMS.MarkedCodesFromReport>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/quality/cisList?reportId={reportId}&omsId={_omsId}",
                headerData, "application/json");

            return result;
        }

        public Models.OMS.OrdersList GetOrdersList()
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var result = _webService.GetRequest<Models.OMS.OrdersList>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/order/list?omsId={_omsId}",
                headerData, "application/json");

            return result;
        }

        public Models.OMS.BufferInfo[] GetOrderStatus(string orderId, string gtin = null)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            string url = $"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/order/status?omsId={_omsId}&orderId={orderId}";

            if (!string.IsNullOrEmpty(gtin))
                url = $"{url}&gtin={gtin}";

            var result = _webService.GetRequest<Models.OMS.BufferInfo[]>(url, headerData, "application/json");
            return result;
        }

        public Dictionary<string, Models.OMS.Product> GetProductListFromOrder(string orderId)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var result = _webService.GetRequest<Dictionary<string, Models.OMS.Product>>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/order/product?omsId={_omsId}&orderId={orderId}",
                headerData, "application/json");

            return result;
        }

        public Models.OMS.MarkedCodes GetMarkedCodes(string orderId, int quantity, string gtin)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var result = _webService.GetRequest<Models.OMS.MarkedCodes>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/codes?orderId={orderId}&gtin={gtin}&quantity={quantity}&omsId={_omsId}",
                headerData, "application/json");

            return result;
        }

        public Models.OMS.MarkedCodes GetMarkedCodesRetry(string blockId)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var result = _webService.GetRequest<Models.OMS.MarkedCodes>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/order/codes/retry?omsId={_omsId}&blockId={blockId}",
                headerData, "application/json");

            return result;
        }

        public Models.OMS.ReportForApplicationResponse SendReportForApplication(string productFroup, List<string> codes)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var reportRequest = new Models.OMS.ReportForApplicationRequest
            {
                ProductGroup = productFroup,
                Sntins = codes.ToArray()
            };
            var reportRequestAsJson = JsonConvert.SerializeObject(reportRequest);

            var crypto = new WinApiCryptWrapper(_certificate);
            byte[] signedData = Encoding.UTF8.GetBytes(reportRequestAsJson);
            var signature = crypto.Sign(signedData, true);

            headerData.Add("X-Signature", Convert.ToBase64String(signature));

            var result = _webService.PostRequest<Models.OMS.ReportForApplicationResponse>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/utilisation?omsId={_omsId}",
                reportRequestAsJson, null, "application/json", headerData, "application/json");

            return result;
        }

        public Models.OMS.Base.FilterSearchResult<string> GetApplyLabelReportsIds(string orderId = null, int? limit = null, int? skip = null)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var reference = $"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/quality?omsId={_omsId}";

            if (!string.IsNullOrEmpty(orderId))
                reference = $"{reference}&orderId={orderId}";

            if(limit != null)
                reference = $"{reference}&limit={limit}";

            if (skip != null)
                reference = $"{reference}&skip={skip}";

            var result = _webService.GetRequest<Models.OMS.Base.FilterSearchResult<string>>(reference, headerData, "application/json");

            return result;
        }

        public Models.OMS.DocumentContent GetDocumentContent(string docId)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var reference = $"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/documents/content?omsId={_omsId}&docId={docId}";

            var result = _webService.GetRequest<Models.OMS.DocumentContent>(reference, headerData, "application/json");

            return result;
        }

        public Models.OMS.DocumentContent GetDocumentByReceipt(string resultDocId, string docId)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var reference = $"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/receipts/document?resultDocId={resultDocId}&docId={docId}&omsId={_omsId}";

            var result = _webService.GetRequest<Models.OMS.DocumentContent>(reference, headerData, "application/json");

            return result;
        }

        public Models.OMS.Receipt GetReceiptContent(string resultDocId)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var reference = $"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/receipts/receipt?omsId={_omsId}&resultDocId={resultDocId}";

            var responseStr = _webService.GetRequest(reference, headerData, null, "application/json");

            var responseJson = JsonConvert.DeserializeObject(responseStr) as Newtonsoft.Json.Linq.JObject;
            var resultArrayJson = responseJson["results"] as Newtonsoft.Json.Linq.JArray;
            var resultJson = resultArrayJson.FirstOrDefault(o => (o["resultDocId"] as Newtonsoft.Json.Linq.JValue)?.Value as string == resultDocId);

            var result = resultJson.ToObject<Models.OMS.Receipt>();
            return result;
        }

        public Models.OMS.Base.FilterSearchResult<Models.OMS.ResultSearch> SearchDocuments(string documentType, int? skip = null, int? limit = null)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var reference = $"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/documents/search?omsId={_omsId}&documentType={documentType}";

            if (skip != null)
                reference = reference + $"&skip={skip}";

            if (limit != null)
                reference = reference + $"&limit={limit}";

            var result = _webService.GetRequest<Models.OMS.Base.FilterSearchResult<Models.OMS.ResultSearch>>(reference, headerData, "application/json");

            return result;
        }

        public Models.OMS.Base.FilterSearchResult<Models.OMS.Receipt> SearchReceipts(string[] workFlowTypes = null, string[] productGroups = null, int[] statuses = null, string[] orderIds = null, 
            string[] sourceDocIds = null, string[] resultDocIds = null, DateTime? startCreateDoc = null, DateTime? endCreateDoc = null, int? skip = null, int? limit = null)
        {
            var headerData = new Dictionary<string, string>();
            headerData.Add("clientToken", _token);

            var filter = new Models.OMS.FilterSearchReceiptsRequest
            {
                WorkflowTypes = workFlowTypes,
                ProductGroups = productGroups,
                ResultCodes = statuses,
                OrderIds = orderIds,
                SourceDocIds = sourceDocIds,
                ResultDocIds = resultDocIds
            };

            if (startCreateDoc != null)
            {
                filter.StartCreateDocDate = (startCreateDoc.Value.Ticks - 621355968000000000) / 10000;

                if (endCreateDoc == null)
                    throw new Exception("Не указана конечная дата интервала поиска");

                filter.EndCreateDocDate = (endCreateDoc.Value.Ticks - 621355968000000000) / 10000;

                if (filter.EndCreateDocDate < filter.StartCreateDocDate)
                    throw new Exception("Дата окончания интервала меньше даты начала.");

                if (filter.EndCreateDocDate - filter.StartCreateDocDate > 604800000)
                    throw new Exception("Интервал дат превышает допустимое значение");
            }

            var jsonObj = new Newtonsoft.Json.Linq.JObject();
            jsonObj.Add("filter", Newtonsoft.Json.Linq.JObject.FromObject(filter));

            if(limit != null)
                jsonObj.Add("limit", new Newtonsoft.Json.Linq.JValue(limit.Value));

            if(skip != null)
                jsonObj.Add("skip", new Newtonsoft.Json.Linq.JValue(skip.Value));

            var requestStr = JsonConvert.SerializeObject(jsonObj);
            var response = _webService.PostRequest<Models.OMS.Base.FilterSearchResult<Models.OMS.Receipt>>($"{Properties.Settings.Default.UrlAddressOrderManagmentStation}/api/v3/receipts/receipt/search?omsId={_omsId}",
                requestStr, null, "application/json", headerData, "application/json");

            return response;
        }

        public bool Authorization(X509Certificate2 certificate, string omsConnection, string omsId, string emchdOrgInn = null)
        {
            OrderManagementStationTokenCache cache;

            if (!string.IsNullOrEmpty(emchdOrgInn))
                cache = new OrderManagementStationTokenCache().Load($"{emchdOrgInn}_{certificate.Thumbprint}");
            else
                cache = new OrderManagementStationTokenCache().Load(certificate.Thumbprint);

            if (cache?.Token != null && cache?.TokenExpirationDate > DateTime.Now)
            {
                _token = cache.Token;
                _certificate = certificate;
                _omsId = omsId;
                return true;
            }

            var authData = _webService.GetRequest<Models.AuthData>($"{Properties.Settings.Default.UrlAddressHonestMark}/auth/key");

            var crypto = new WinApiCryptWrapper(certificate);

            byte[] signedData = Encoding.UTF8.GetBytes(authData.Data);
            var signature = crypto.Sign(signedData, false);

            var authRequest = new Models.AuthRequest
            {
                Uid = authData.Uid,
                Data = Convert.ToBase64String(signature)
            };

            if (!string.IsNullOrEmpty(emchdOrgInn))
                authRequest.Inn = emchdOrgInn;

            var authRequestJson = JsonConvert.SerializeObject(authRequest);

            var result = _webService.PostRequest<Models.AuthResult>($"{Properties.Settings.Default.UrlAddressHonestMark}/auth/simpleSignIn/{omsConnection}",
                authRequestJson, null, "application/json");

            if (result.ErrorMessage != null)
                throw new Exception($"Произошла ошибка с кодом {result.ErrorCode}:{result.ErrorMessage} /nОписание:{result.Description}");

            if (string.IsNullOrEmpty(result.Token))
                throw new Exception("Не удалось получить токен авторизации.");

            _token = result.Token;

            cache = new OrderManagementStationTokenCache()
            {
                Token = result.Token,
                TokenCreationDate = DateTime.Now,
                TokenExpirationDate = DateTime.Now.AddHours(10)
            };

            if (!string.IsNullOrEmpty(emchdOrgInn))
                cache.Save(cache, $"{emchdOrgInn}_{certificate.Thumbprint}");
            else
                cache.Save(cache, certificate.Thumbprint);

            _certificate = certificate;
            _omsId = omsId;

            return true;
        }
    }
}
