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
        /// <param name="url">The target url.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static string Get(string url, Action<HttpWebRequest> modifyRequest = null)
        {
            using (var c = new GZipWebClient(modifyRequest))
                return c.DownloadString(url);
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static T Get<T>(string url, Action<HttpWebRequest> modifyRequest = null)
        {
            return Get(url, modifyRequest).Deserialize<T>();
        }

        /// <summary>
        /// Sends a GET request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static Task<string> GetAsync(string url, Action<HttpWebRequest> modifyRequest = null)
        {
            return Task.Factory.StartNew(() => Get(url, modifyRequest));
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static Task<T> GetAsync<T>(string url, Action<HttpWebRequest> modifyRequest = null)
        {
            return Task.Factory.StartNew(() => Get<T>(url, modifyRequest));
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static string Post(string url, string data, Action<HttpWebRequest> modifyRequest = null)
        {
            using (var c = new GZipWebClient(modifyRequest))
                return c.UploadString(url, data);
        }

        /// <summary>
        /// Sends a POST request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static T Post<T>(string url, string data, Action<HttpWebRequest> modifyRequest = null)
        {
            return Post(url, data, modifyRequest).Deserialize<T>();
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static Task<string> PostAsync(string url, string data, Action<HttpWebRequest> modifyRequest = null)
        {
            return Task.Factory.StartNew(() => Post(url, data, modifyRequest));
        }

        /// <summary>
        /// Sends a POST request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="modifyRequest">An action that modifies the request.  Use this to add custom headers, etc.</param>
        /// <returns></returns>
        public static Task<T> PostAsync<T>(string url, string data, Action<HttpWebRequest> modifyRequest = null)
        {
            return Task.Factory.StartNew(() => Post<T>(url, data, modifyRequest));
        }

        private class GZipWebClient : WebClient
        {
            private readonly Action<HttpWebRequest> _modifyRequest;

            public GZipWebClient(Action<HttpWebRequest> modifyRequest = null)
            {
                _modifyRequest = modifyRequest;

                Encoding = Encoding.UTF8;
                Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address) as HttpWebRequest;

                if (request == null)
                    return null;

                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.ServicePoint.Expect100Continue = false;

                if (_modifyRequest != null)
                    _modifyRequest(request);

                return request;
            }
        }
    }
}
