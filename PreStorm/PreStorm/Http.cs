using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PreStorm
{
    /// <summary>
    /// Encapsulates HTTP calls to ArcGIS Server.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Sends a GET request and returns the response body.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static string Get(string url, ICredentials credentials = null)
        {
            using (var c = new GZipWebClient(credentials))
                return c.DownloadString(url);
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static T Get<T>(string url, ICredentials credentials = null)
        {
            return Get(url, credentials).Deserialize<T>();
        }

        /// <summary>
        /// Sends a GET request and returns the response body.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static Task<string> GetAsync(string url, ICredentials credentials = null)
        {
            return Task.Factory.StartNew(() => Get(url, credentials));
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static Task<T> GetAsync<T>(string url, ICredentials credentials = null)
        {
            return Task.Factory.StartNew(() => Get<T>(url, credentials));
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static string Post(string url, string data, ICredentials credentials = null)
        {
            using (var c = new GZipWebClient(credentials))
                return c.UploadString(url, data);
        }

        /// <summary>
        /// Sends a POST request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static T Post<T>(string url, string data, ICredentials credentials = null)
        {
            return Post(url, data, credentials).Deserialize<T>();
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static Task<string> PostAsync(string url, string data, ICredentials credentials = null)
        {
            return Task.Factory.StartNew(() => Post(url, data, credentials));
        }

        /// <summary>
        /// Sends a POST request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static Task<T> PostAsync<T>(string url, string data, ICredentials credentials = null)
        {
            return Task.Factory.StartNew(() => Post<T>(url, data, credentials));
        }

        private class GZipWebClient : WebClient
        {
            private readonly ICredentials _credentials;

            public GZipWebClient(ICredentials credentials)
            {
                _credentials = credentials;

                Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                Encoding = Encoding.UTF8;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address) as HttpWebRequest;

                if (request == null)
                    return null;

                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Credentials = _credentials;
                request.ServicePoint.Expect100Continue = false;
                return request;
            }
        }
    }
}
