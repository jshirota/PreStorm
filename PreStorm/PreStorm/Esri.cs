using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace PreStorm
{
    internal static class Esri
    {
        private static T GetResponse<T>(string url, string data, ICredentials credentials, Token token) where T : Response
        {
            var json = data == null
                ? Http.Get(string.Format("{0}{1}token={2}&f=json", url, url.Contains("?") ? "&" : "?", token), credentials)
                : Http.Post(url, string.Format("{0}&token={1}&f=json", data, token), credentials);

            var response = json.Deserialize<T>();

            if (response.error != null)
                throw new Exception(string.Join("  ", new[] { response.error.message }.Concat(response.error.details)));

            return response;
        }

        private static readonly Func<string, ICredentials, Token, ServiceInfo> GetServiceInfoMemoized = Memoization.Memoize<string, ICredentials, Token, ServiceInfo>((u, c, t) => GetResponse<ServiceInfo>(u, null, c, t));

        public static ServiceInfo GetServiceInfo(string url, ICredentials credentials, Token token)
        {
            var url2 = Regex.Replace(url, @"/FeatureServer($|/)", "/MapServer", RegexOptions.IgnoreCase) + "/layers";

            return GetServiceInfoMemoized(url2, credentials, token);
        }

        public static OIDSet GetOIDSet(string url, int layerId, ICredentials credentials, Token token, string whereClause)
        {
            var url2 = url + "/" + layerId + "/query";
            var data = string.Format("where={0}&returnIdsOnly=true",
                HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause));

            return GetResponse<OIDSet>(url2, data, credentials, token);
        }

        public static FeatureSet GetFeatureSet(string url, int layerId, ICredentials credentials, Token token, bool returnGeometry, string whereClause, IEnumerable<int> objectIds)
        {
            var url2 = url + "/" + layerId + "/query";
            var data = string.Format("where={0}&objectIds={1}&returnGeometry={2}&outFields=*",
                HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause),
                objectIds == null ? "" : HttpUtility.UrlEncode(string.Join(",", objectIds)),
                returnGeometry ? "true" : "false");

            return GetResponse<FeatureSet>(url2, data, credentials, token);
        }

        public static TokenInfo GetTokenInfo(string url, string userName, string password)
        {
            var url2 = string.Format("{0}/tokens/generateToken?userName={1}&password={2}&clientid=requestip",
                Regex.Match(url, @"^http.*?(?=(/rest/services/))", RegexOptions.IgnoreCase).Value, userName, password);

            return GetResponse<TokenInfo>(url2, null, null, null);
        }

        public static EditResultInfo ApplyEdits(string url, Layer layer, ICredentials credentials, Token token, string operation, string json)
        {
            var url2 = string.Format("{0}/{1}/applyEdits", url, layer.id);
            var data = string.Format("{0}={1}", operation, HttpUtility.UrlEncode(json));

            return GetResponse<EditResultInfo>(url2, data, credentials, token);
        }

        #region Esri REST API

        public class Response
        {
            public Error error { get; set; }
        }

        public class Error
        {
            public int code { get; set; }
            public string message { get; set; }
            public string[] details { get; set; }
        }

        public class ServiceInfo : Response
        {
            public Layer[] layers { get; set; }
            public Layer[] tables { get; set; }
        }

        public class EditResultInfo : Response
        {
            public EditResult[] addResults { get; set; }
            public EditResult[] updateResults { get; set; }
            public EditResult[] deleteResults { get; set; }
        }

        public class EditResult
        {
            public int objectId { get; set; }
            public bool success { get; set; }
        }

        public class TokenInfo : Response
        {
            public string token { get; set; }
            public long expires { get; set; }
        }

        public class OIDSet : Response
        {
            public string objectIdFieldName { get; set; }
            public int[] objectIds { get; set; }
        }

        public class FeatureSet : Response
        {
            public Graphic[] features { get; set; }
        }

        public class Graphic
        {
            public Dictionary<string, object> attributes { get; set; }
            public Geometry geometry { get; set; }
        }

        public class Geometry
        {
            public double x { get; set; }
            public double y { get; set; }
            public double[][] points { get; set; }
            public double[][][] paths { get; set; }
            public double[][][] rings { get; set; }
        }

        #endregion
    }
}
