using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PreStorm
{
    /// <summary>
    /// Encapsulates HTTP calls to ArcGIS Server.
    /// </summary>
    public class Http : WebClient
    {
        /// <summary>
        /// An action that globally modifies the underlying HttpWebRequest object.
        /// </summary>
        public static Action<HttpWebRequest> RequestModifier { get; set; }

        private readonly Action<HttpWebRequest> _requestModifier;

        /// <summary>
        /// Initializes a new instance of the Http class.
        /// </summary>
        /// <param name="requestModifier">An action that modifies the request.</param>
        public Http(Action<HttpWebRequest> requestModifier = null)
        {
            _requestModifier = requestModifier;

            Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Overridden to use gzip compression.  The request is further modified using the requestModifier action set via the constructor.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;

            if (request == null)
                return null;

            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.ServicePoint.Expect100Continue = false;

            var requestModifier = RequestModifier;

            if (requestModifier != null)
                requestModifier(request);

            if (_requestModifier != null)
                _requestModifier(request);

            return request;
        }

        /// <summary>
        /// Sends a GET request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static string Get(string url, Action<HttpWebRequest> requestModifier = null)
        {
            using (var http = new Http(requestModifier))
            {
                return http.DownloadString(url);
            }
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static T Get<T>(string url, Action<HttpWebRequest> requestModifier = null)
        {
            return Get(url, requestModifier).Deserialize<T>();
        }

        /// <summary>
        /// Sends a GET request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static Task<string> GetAsync(string url, Action<HttpWebRequest> requestModifier = null)
        {
            return Task.Factory.StartNew(() => Get(url, requestModifier));
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static Task<T> GetAsync<T>(string url, Action<HttpWebRequest> requestModifier = null)
        {
            return Task.Factory.StartNew(() => Get<T>(url, requestModifier));
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static string Post(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            using (var http = new Http(requestModifier))
            {
                http.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                return http.UploadString(url, data);
            }
        }

        /// <summary>
        /// Sends a POST request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static T Post<T>(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            return Post(url, data, requestModifier).Deserialize<T>();
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static Task<string> PostAsync(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            return Task.Factory.StartNew(() => Post(url, data, requestModifier));
        }

        /// <summary>
        /// Sends a POST request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static Task<T> PostAsync<T>(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            return Task.Factory.StartNew(() => Post<T>(url, data, requestModifier));
        }
    }
}
