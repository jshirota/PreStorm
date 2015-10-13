using System;
using System.IO;
using System.Net;
using System.Text;

namespace PreStorm
{
    /// <summary>
    /// Abstracts HTTP calls to ArcGIS Server.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// An action that globally modifies the underlying HttpWebRequest object.
        /// </summary>
        public static Action<HttpWebRequest> RequestModifier { get; set; }

        private static string GetResponseText(this WebRequest request)
        {
            var response = Compatibility.GetResponse(request);

            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Sends a GET request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static string Get(string url, Action<HttpWebRequest> requestModifier = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            Compatibility.ModifyRequest(request);

            RequestModifier?.Invoke(request);
            requestModifier?.Invoke(request);

            return request.GetResponseText();
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
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static string Post(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            Compatibility.ModifyRequest(request);

            RequestModifier?.Invoke(request);
            requestModifier?.Invoke(request);

            using (var stream = Compatibility.GetRequestStream(request))
            {
                stream.Write(bytes, 0, bytes.Length);
                return request.GetResponseText();
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
    }
}
