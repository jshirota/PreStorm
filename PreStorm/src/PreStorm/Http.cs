using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

        private static async Task<string> GetResponseTextAsync(this WebRequest request)
        {
            var response = await request.GetResponseAsync();

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
        public static async Task<string> GetAsync(string url, Action<HttpWebRequest> requestModifier = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            //request.AutomaticDecompression = DecompressionMethods.GZip;
            //request.ServicePoint.Expect100Continue = false;

            RequestModifier?.Invoke(request);
            requestModifier?.Invoke(request);

            return await request.GetResponseTextAsync();
        }

        /// <summary>
        /// Sends a GET request and returns the response JSON deserialized as the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The target url.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string url, Action<HttpWebRequest> requestModifier = null)
        {
            return (await GetAsync(url, requestModifier)).Deserialize<T>();
        }

        /// <summary>
        /// Sends a POST request and returns the response body.
        /// </summary>
        /// <param name="url">The target url.</param>
        /// <param name="data">The string data to upload.</param>
        /// <param name="requestModifier">An action that modifies the request.</param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

#if !DOTNET
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.ServicePoint.Expect100Continue = false;
#endif

            RequestModifier?.Invoke(request);
            requestModifier?.Invoke(request);

            using (var stream = await request.GetRequestStreamAsync())
            {
                stream.Write(bytes, 0, bytes.Length);
                return await request.GetResponseTextAsync();
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
        public static async Task<T> PostAsync<T>(string url, string data, Action<HttpWebRequest> requestModifier = null)
        {
            return (await PostAsync(url, data, requestModifier)).Deserialize<T>();
        }
    }
}
