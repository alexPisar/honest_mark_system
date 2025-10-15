using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitesLibrary.Service
{
    public class ProxyConnect
    {
        private System.Net.WebProxy _webProxy;
        private string _baseUrlAddress;

        public ProxyConnect(string proxyAddress, string baseUrlAddress)
        {
            _webProxy = new System.Net.WebProxy($"http://{proxyAddress}");
            _baseUrlAddress = baseUrlAddress;
        }

        public bool CheckProxyConnect(string proxyLogin, string proxyPassword)
        {
            _webProxy.Credentials = new System.Net.NetworkCredential(proxyLogin, proxyPassword);

            var handler = new System.Net.Http.HttpClientHandler
            {
                Proxy = _webProxy,
                UseProxy = true
            };

            bool successStatus;
            using (var client = new System.Net.Http.HttpClient(handler))
            {
                try
                {
                    var response = client.GetAsync(_baseUrlAddress).Result;
                    successStatus = response.IsSuccessStatusCode;
                }
                catch
                {
                    successStatus = false;
                }
            }

            return successStatus;
        }
    }
}
