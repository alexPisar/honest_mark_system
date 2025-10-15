using System;
using System.Security.Cryptography.X509Certificates;
using ConfigSet.Configs;
using Cryptography.WinApi;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

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
