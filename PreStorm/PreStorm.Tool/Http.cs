using System;
using System.Net;

namespace PreStorm.Tool
{
    internal static class Http
    {
        public static string Download(string url, ICredentials credentials)
        {
            using (var c = new GZipWebClient(credentials))
                return c.DownloadString(url);
        }

        private class GZipWebClient : WebClient
        {
            private readonly ICredentials _credentials;

            public GZipWebClient(ICredentials credentials)
            {
                _credentials = credentials;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Credentials = _credentials;
                return request;
            }
        }
    }
}
