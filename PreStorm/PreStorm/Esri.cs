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
                ? Http.Get(url, credentials)
                : Http.Post(url, string.Format("{0}&token={1}&f=json", data, token), credentials);

            var response = json.Deserialize<T>();

            if (response.error != null)
                throw new Exception(string.Join("  ", new[] { response.error.message }.Concat(response.error.details)));

            return response;
        }

        private static T GetResponse<T>(string url, ICredentials credentials, Token token) where T : Response
        {
            return GetResponse<T>(string.Format("{0}{1}token={2}&f=json", url, url.Contains("?") ? "&" : "?", token), null, credentials, null);
        }

        private static readonly Func<string, ICredentials, Token, ServiceInfo> GetServiceInfoMemoized = Memoization.Memoize<string, ICredentials, Token, ServiceInfo>(GetResponse<ServiceInfo>);

        public static ServiceInfo GetServiceInfo(string url, ICredentials credentials, Token token)
        {
            return GetServiceInfoMemoized(Regex.Replace(url, @"/FeatureServer($|/)", "/MapServer", RegexOptions.IgnoreCase) + "/layers", credentials, token);
        }

        public static OIDSet GetOIDSet(string url, int layerId, ICredentials credentials, Token token, string whereClause)
        {
            var data = string.Format("where={0}&returnIdsOnly=true",
                HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause));

            return GetResponse<OIDSet>(url + "/" + layerId + "/query", data, credentials, token);
        }

        public static FeatureSet GetFeatureSet(string url, int layerId, ICredentials credentials, Token token, bool returnGeometry, string whereClause, IEnumerable<int> objectIds)
        {
            var data = string.Format("where={0}&objectIds={1}&returnGeometry={2}&outFields=*",
                HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause),
                objectIds == null ? "" : HttpUtility.UrlEncode(string.Join(",", objectIds)),
                returnGeometry ? "true" : "false");

            return GetResponse<FeatureSet>(url + "/" + layerId + "/query", data, credentials, token);
        }

        public static TokenInfo GetTokenInfo(string url, string userName, string password)
        {
            var tokenUrl = string.Format("{0}/tokens/generateToken?userName={1}&password={2}&clientid=requestip",
                Regex.Match(url, @"^http.*?(?=(/rest/services/))", RegexOptions.IgnoreCase).Value, userName, password);

            return GetResponse<TokenInfo>(tokenUrl, null, null);
        }

        public static EditResultInfo ApplyEdits(string url, Layer layer, ICredentials credentials, Token token, string operation, string json)
        {
            if (url == null || layer == null)
                throw new Exception("The features cannot be edited because they are not bound to a layer.");

            var u = string.Format("{0}/{1}/applyEdits", url, layer.id);
            var data = string.Format("{0}={1}", operation, HttpUtility.UrlEncode(json));

            return GetResponse<EditResultInfo>(u, data, credentials, token);
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

        public class Layer
        {
            public double currentVersion { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public Field[] fields { get; set; }
        }

        public class Field
        {
            public string name { get; set; }
            public string type { get; set; }
            public Domain domain { get; set; }
        }

        public class Domain
        {
            public string type { get; set; }
            public string name { get; set; }
            public CodedValue[] codedValues { get; set; }
        }

        public class CodedValue
        {
            public string name { get; set; }
            public object code { get; set; }
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

        #region Esri REST API Helper

        public static string GetObjectIdFieldName(this Layer layer)
        {
            var objectIdFields = layer.fields.Where(f => f.type == "esriFieldTypeOID").ToArray();

            if (objectIdFields.Length != 1)
                throw new Exception("Layer must have one and only one field of type esriFieldTypeOID.");

            return objectIdFields.Single().name;
        }

        private static CodedValue[] GetCodeValues(this Layer layer, string domainName)
        {
            var domain = layer.fields.Select(f => f.domain).FirstOrDefault(d => d != null && d.type == "codedValue" && d.name == domainName);

            if (domain == null)
                throw new Exception(string.Format("Coded value domain '{0}' does not exist.", domainName));

            return domain.codedValues;
        }

        public static CodedValue GetCodedValueByCode(this Layer layer, string domainName, object code)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.code.ToString() == code.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
                throw new Exception(string.Format("Coded value domain '{0}' does not contain code '{1}'.", domainName, code));

            throw new Exception(string.Format("Coded value domain '{0}' contains {1} occurrences of code '{2}'.", domainName, codedValues.Length, code));
        }

        public static CodedValue GetCodedValueByName(this Layer layer, string domainName, object name)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.name == name.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
                throw new Exception(string.Format("Coded value domain '{0}' does not contain name '{1}'.", domainName, name));

            throw new Exception(string.Format("Coded value domain '{0}' contains {1} occurrences of name '{2}'.", domainName, codedValues.Length, name));
        }

        #endregion
    }
}
