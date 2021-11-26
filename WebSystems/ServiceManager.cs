using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

namespace WebSystems
{
    public class ServiceManager
    {
        private WebProxy _webProxy = null;
        private string _statusCode = null;

        public ServiceManager() { }

        public ServiceManager(string proxyAddress, string proxyUserName, string proxyPassword)
        {
            _webProxy = new WebProxy();

            _webProxy.Address = new Uri("http://" + proxyAddress);
            _webProxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);
        }

        public string GetStatusCode()
        {
            return _statusCode;
        }

        public string PostRequest(string url, object contentData, string cookie = null, string contentType = null,
            Dictionary<string, string> headers = null, Encoding encoding = null)
        {
            if (contentType == "multipart/form-data")
            {
                var multipartContentData = contentData as Dictionary<object, string>;

                var requestContent = new System.Net.Http.MultipartFormDataContent();
                var fileStreamList = new List<System.IO.FileStream>();

                try
                {
                    foreach (var parameter in multipartContentData)
                    {
                        if (parameter.Key?.GetType() == typeof(Models.FileParameter))
                        {
                            var fileParameter = parameter.Key as Models.FileParameter;

                            System.Net.Http.HttpContent fileContent;
                            var stream = new System.IO.FileStream(parameter.Value, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                            fileStreamList.Add(stream);
                            fileContent = new System.Net.Http.StreamContent(stream);

                            string fileName = string.IsNullOrEmpty(fileParameter.FileName) 
                                ? parameter.Value : fileParameter.FileName;

                            fileContent.Headers.ContentDisposition = 
                                new System.Net.Http.Headers.ContentDispositionHeaderValue(fileParameter.ContentDispositionType)
                                {
                                    Name = fileParameter.Name,
                                    FileName = fileName
                                };

                            fileContent.Headers.ContentType = 
                                System.Net.Http.Headers.MediaTypeHeaderValue.Parse(fileParameter.ContentType);

                            requestContent.Add(fileContent, fileParameter.Name, fileName);
                            
                        }
                        else if (parameter.Key?.GetType() == typeof(string))
                        {
                            var stringContent = new System.Net.Http.StringContent(parameter.Value);
                            requestContent.Add(stringContent, (string)parameter.Key);
                        }
                    }

                    System.Net.Http.HttpClient client;
                    if (_webProxy != null)
                    {
                        System.Net.Http.HttpClientHandler httpClientHandler = new System.Net.Http.HttpClientHandler()
                        {
                            Proxy = _webProxy,
                            PreAuthenticate = true,
                            UseDefaultCredentials = false,
                        };

                        client = new System.Net.Http.HttpClient(httpClientHandler);
                    }
                    else
                        client = new System.Net.Http.HttpClient();

                    if (headers != null)
                        foreach (var header in headers)
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);

                    var res = client.PostAsync(url, requestContent).Result;
                    _statusCode = ((int)res.StatusCode).ToString();
                    return res.Content.ReadAsStringAsync().Result;
                }
                finally
                {
                    foreach (var fileStream in fileStreamList)
                        fileStream.Close();
                }
            }
            else
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";

                if (string.IsNullOrEmpty(contentType))
                    request.ContentType = "application/x-www-form-urlencoded";
                else
                    request.ContentType = contentType;

                if (_webProxy != null)
                    request.Proxy = _webProxy;

                if (cookie != null)
                    request.Headers.Add("Cookie", $"{cookie}");

                if (headers != null)
                {
                    foreach (var header in headers)
                        request.Headers.Add(header.Key, header.Value);
                }

                var requestStream = request.GetRequestStream();

                var contentDataBytes = System.Text.Encoding.UTF8.GetBytes(contentData as string);

                requestStream.Write(contentDataBytes, 0, contentDataBytes.Length);

                var response = request.GetResponse();

                string result;

                if (encoding != null)
                {
                    using (var sr = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                    {
                        result = sr.ReadToEnd();
                    }
                }
                else
                {
                    using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        result = sr.ReadToEnd();
                    }
                }

                _statusCode = ((int?)((HttpWebResponse)response)?.StatusCode)?.ToString();
                return result;
            }
        }

        public T PostRequest<T>(string url, string contentData, string cookie = null, string contentType = null,
            Dictionary<string, string> headers = null)
        {
            string resultAsJsonStr = PostRequest(url, contentData, cookie, contentType, headers);

            var result = JsonConvert.DeserializeObject<T>(resultAsJsonStr);

            return result;
        }

        public string GetRequest(string url, Dictionary<string, string> headers = null, Encoding encoding = null, string accept = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";

            if(accept != null)
                request.Accept = accept;

            if (headers != null)
            {
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);
            }

            if (_webProxy != null)
                request.Proxy = _webProxy;

            var response = request.GetResponse();

            string result;

            if (encoding != null)
            {
                using (var sr = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                {
                    result = sr.ReadToEnd();
                }
            }
            else
            {
                using (var sr = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }

            return result;
        }

        public T GetRequest<T>(string url, Dictionary<string, string> headers = null)
        {
            string resultAsJsonStr = GetRequest(url, headers);

            var result = JsonConvert.DeserializeObject<T>(resultAsJsonStr);

            return result;
        }

        public byte[] DownloadDataFileFromReference(string url)
        {
            System.Net.WebClient client = new System.Net.WebClient();

            return client.DownloadData(url);
        }
    }
}
