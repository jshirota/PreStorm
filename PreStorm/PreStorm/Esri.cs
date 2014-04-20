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
        private static T GetResponse<T>(string url, string data, ICredentials credentials, Token token, string gdbVersion) where T : Response
        {
            var parameters = new Dictionary<string, object>
            {
                {"token", token},
                {"gdbVersion", gdbVersion},
                {"f", "json"}
            };

            var queryString = string.Join("&", parameters.Where(o => o.Value != null).Select(o => string.Format("{0}={1}", o.Key, o.Value)));

            var json = data == null
                ? Http.Get(String.Format("{0}{1}{2}", url, url.Contains("?") ? "&" : "?", queryString), credentials)
                : Http.Post(url, String.Format("{0}&{1}", data, queryString), credentials);

            var response = json.Deserialize<T>();

            if (response.error != null)
                throw new Exception(response.error.message);

            return response;
        }

        private static readonly Func<ServiceIdentity, ServiceInfo> GetServiceInfoMemoized = Memoization.Memoize<ServiceIdentity, ServiceInfo>(i =>
        {
            var url = Regex.Replace(i.Url, @"/FeatureServer($|/)", i.IsArcGISOnline ? "/FeatureServer" : "/MapServer", RegexOptions.IgnoreCase) + "/layers";

            return GetResponse<ServiceInfo>(url, null, i.Credentials, i.Token, i.GdbVersion);
        });

        public static ServiceInfo GetServiceInfo(ServiceIdentity identity)
        {
            return GetServiceInfoMemoized(identity);
        }

        public static OIDSet GetOIDSet(ServiceIdentity identity, int layerId, string whereClause)
        {
            var url = identity.Url + "/" + layerId + "/query";
            var data = String.Format("where={0}&returnIdsOnly=true",
                HttpUtility.UrlEncode(String.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause));

            return GetResponse<OIDSet>(url, data, identity.Credentials, identity.Token, identity.GdbVersion);
        }

        public static FeatureSet GetFeatureSet(ServiceIdentity identity, int layerId, bool returnGeometry, string whereClause, IEnumerable<int> objectIds)
        {
            var url = identity.Url + "/" + layerId + "/query";
            var data = String.Format("where={0}&objectIds={1}&returnGeometry={2}&outFields=*",
                HttpUtility.UrlEncode(String.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause),
                objectIds == null ? "" : HttpUtility.UrlEncode(String.Join(",", objectIds)),
                returnGeometry ? "true" : "false");

            return GetResponse<FeatureSet>(url, data, identity.Credentials, identity.Token, identity.GdbVersion);
        }

        public static TokenInfo GetTokenInfo(string url, string userName, string password)
        {
            var url2 = String.Format("{0}/tokens/generateToken?userName={1}&password={2}&clientid=requestip",
                Regex.Match(url, @"^http.*?(?=(/rest/services/))", RegexOptions.IgnoreCase).Value, userName, password);

            return GetResponse<TokenInfo>(url2, null, null, null, null);
        }

        public static ArcGISOnlineTokenInfo GetArcGISOnlineTokenInfo(string clientId, string clientSecret)
        {
            var url = String.Format("https://www.arcgis.com/sharing/oauth2/token?client_id={0}&grant_type=client_credentials&client_secret={1}&f=json", clientId, clientSecret);

            return GetResponse<ArcGISOnlineTokenInfo>(url, null, null, null, null);
        }

        public static EditResultSet ApplyEdits(ServiceIdentity identity, int layerId, string operation, string json)
        {
            var url = String.Format("{0}/{1}/applyEdits", identity.Url, layerId);
            var data = String.Format("{0}={1}", operation, HttpUtility.UrlEncode(json));

            return GetResponse<EditResultSet>(url, data, identity.Credentials, identity.Token, identity.GdbVersion);
        }

        public static string GetObjectIdFieldName(this Layer layer)
        {
            var objectIdFields = layer.fields.Where(f => f.type == "esriFieldTypeOID").ToArray();

            if (objectIdFields.Length != 1)
                throw new Exception("Layer must have one and only one field of type esriFieldTypeOID.");

            return objectIdFields.Single().name;
        }

        private static IEnumerable<CodedValue> GetCodeValues(this Layer layer, string domainName)
        {
            var domain = layer.fields.Select(f => f.domain).FirstOrDefault(d => d != null && d.type == "codedValue" && d.name == domainName);

            if (domain == null)
                throw new Exception(String.Format("Coded value domain '{0}' does not exist.", domainName));

            return domain.codedValues;
        }

        public static CodedValue GetCodedValueByCode(this Layer layer, string domainName, object code)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.code.ToString() == code.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
                throw new Exception(String.Format("Coded value domain '{0}' does not contain code '{1}'.", domainName, code));

            throw new Exception(String.Format("Coded value domain '{0}' contains {1} occurrences of code '{2}'.", domainName, codedValues.Length, code));
        }

        public static CodedValue GetCodedValueByName(this Layer layer, string domainName, object name)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.name == name.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
                throw new Exception(String.Format("Coded value domain '{0}' does not contain name '{1}'.", domainName, name));

            throw new Exception(String.Format("Coded value domain '{0}' contains {1} occurrences of name '{2}'.", domainName, codedValues.Length, name));
        }
    }

    #region ArcGIS Rest API

    #region Public

    /// <summary>
    /// Represents the error object as defined in the ArcGIS Rest API.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// The error code.
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// The error details.
        /// </summary>
        public string[] details { get; set; }
    }

    /// <summary>
    /// Represents the layer object as defined in the ArcGIS Rest API.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// The layer ID.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// The name of the layer.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The type of the layer.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The fields of the layer.
        /// </summary>
        public Field[] fields { get; set; }
    }

    /// <summary>
    /// Represents the field object as defined in the ArcGIS Rest API.
    /// </summary>
    public class Field
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The type of the field.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The domain this field depends on.
        /// </summary>
        public Domain domain { get; set; }
    }

    /// <summary>
    /// Represents the domain object as defined in the ArcGIS Rest API.
    /// </summary>
    public class Domain
    {
        /// <summary>
        /// The type of the domain.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The name of the domain.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The coded values.
        /// </summary>
        public CodedValue[] codedValues { get; set; }
    }

    /// <summary>
    /// Represents the coded value object as defined in the ArcGIS Rest API.
    /// </summary>
    public class CodedValue
    {
        /// <summary>
        /// The name of the coded value.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The actual value stored in the database.
        /// </summary>
        public object code { get; set; }
    }

    /// <summary>
    /// Represents the edit result object as defined in the ArcGIS Rest API.
    /// </summary>
    public class EditResult
    {
        /// <summary>
        /// The Object ID of the feature.
        /// </summary>
        public int objectId { get; set; }

        /// <summary>
        /// The Global ID of the feature.
        /// </summary>
        public string globalId { get; set; }

        /// <summary>
        /// Indicates if the edit was successful.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Any error that occurred during the edit.
        /// </summary>
        public Error error { get; set; }
    }

    #endregion

    #region Internal

    internal class Response
    {
        public Error error { get; set; }
    }

    internal class ServiceInfo : Response
    {
        public Layer[] layers { get; set; }
        public Layer[] tables { get; set; }
    }

    internal class TokenInfo : Response
    {
        public string token { get; set; }
        public long expires { get; set; }
    }

    internal class ArcGISOnlineTokenInfo : Response
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }

    internal class OIDSet : Response
    {
        public string objectIdFieldName { get; set; }
        public int[] objectIds { get; set; }
    }

    internal class FeatureSet : Response
    {
        public Graphic[] features { get; set; }
    }

    internal class EditResultSet : Response
    {
        public EditResult[] addResults { get; set; }
        public EditResult[] updateResults { get; set; }
        public EditResult[] deleteResults { get; set; }
    }

    internal class Graphic
    {
        public Dictionary<string, object> attributes { get; set; }
        public CatchAllGeometry geometry { get; set; }
    }

    internal class CatchAllGeometry
    {
        public double? x { get; set; }
        public double? y { get; set; }
        public double[][] points { get; set; }
        public double[][][] paths { get; set; }
        public double[][][] rings { get; set; }
    }

    #endregion

    #endregion
}
