using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using WebSystems.Models.FinDb;

namespace WebSystems.WebClients
{
    public class FinDbWebClient : WebClientSingleInstance<HonestMarkClient>
    {
        private static FinDbWebClient _instance;
        private ServiceManager _serviceManager;
        private string _cipherPassword => WebUtility.UrlEncode("dcw2bcwl3\u0012sehxshm^92ur\\2iK2bz3U3j3dKzquulroh3xpr j");
        private string _urlAddress => $"http://{Properties.Settings.Default.DbWebIpAddress}/upd/edi";

        private FinDbWebClient()
        {
            _serviceManager = new ServiceManager();
        }

        private static readonly object _syncRoot = new object();
        public new static FinDbWebClient GetInstance()
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new FinDbWebClient();
                    }
                }
            }

            return _instance;
        }

        private string FinDbWebRequest(string urlPath, string getContent = null, CookieCollection cookies = null, string contentData = null)
        {
            if(cookies == null)
                cookies = new CookieCollection();
            cookies.Add(new Cookie("position", "5") { Domain = Properties.Settings.Default.DbWebIpAddress });
            cookies.Add(new Cookie("shift", "17") { Domain = Properties.Settings.Default.DbWebIpAddress });

            if(string.IsNullOrEmpty(contentData))
                contentData = $"authorization={_cipherPassword}";
            else
                contentData = $"authorization={_cipherPassword}&{contentData}";

            if (string.IsNullOrEmpty(getContent))
                getContent = "user=edi";
            else
                getContent = $"{getContent}&user=edi";

            return _serviceManager.PostRequest($"{_urlAddress}{urlPath}?{getContent}", contentData, cookies);
        }

        private T FinDbWebRequest<T>(string urlPath, string getContent = null, CookieCollection cookies = null, string contentData = null)
        {
            if (cookies == null)
                cookies = new CookieCollection();
            cookies.Add(new Cookie("position", "5") { Domain = Properties.Settings.Default.DbWebIpAddress });
            cookies.Add(new Cookie("shift", "17") { Domain = Properties.Settings.Default.DbWebIpAddress });

            if (string.IsNullOrEmpty(contentData))
                contentData = $"authorization={_cipherPassword}";
            else
                contentData = $"authorization={_cipherPassword}&{contentData}";

            if (string.IsNullOrEmpty(getContent))
                getContent = "user=edi";
            else
                getContent = $"{getContent}&user=edi";

            return _serviceManager.PostRequest<T>($"{_urlAddress}{urlPath}?{getContent}", contentData, cookies);
        }

        public FinDbDocumentComissionInfo GetComissionDocInfoByIdDocJournal(decimal idDoc)
        {
            return FinDbWebRequest<FinDbDocumentComissionInfo>("/get/document/comission/index.php", $"TraderDocId={idDoc}");
        }

        public T GetApplicationConfigParameter<T>(string appName, string ParameterName)
        {
            var obj = _serviceManager.GetRequest<Newtonsoft.Json.Linq.JObject>($"{_urlAddress}/settings/{appName}/Settings.json", Encoding.GetEncoding(1251));
            var result = obj[ParameterName].ToObject<T>();
            return result;
        }
    }
}
