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
        public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static T GetResponse<T>(string url, string data, ICredentials credentials, Token token, string gdbVersion) where T : Response
        {
            var parameters = new Dictionary<string, object> { { "token", token }, { "gdbVersion", gdbVersion }, { "f", "json" } };
            var queryString = string.Join("&", parameters.Where(p => p.Value != null).Select(p => string.Format("{0}={1}", p.Key, HttpUtility.UrlEncode(p.Value.ToString()))));

            var isPost = data != null;

            var url2 = isPost ? url : (url + (url.Contains("?") ? "&" : "?") + queryString);
            var requestText = isPost ? data + "&" + queryString : "";

            string responseText = null;

            try
            {
                responseText = isPost ? Http.Post(url2, requestText, credentials) : Http.Get(url2, credentials);

                var response = responseText.Deserialize<T>();
                var errorMessage = "ArcGIS Server returned an error response.";

                if (response.error != null)
                    throw new Exception(errorMessage);

                var resultSet = response as EditResultSet;

                if (resultSet != null && new[] { resultSet.addResults, resultSet.updateResults, resultSet.deleteResults }.Any(results => results == null || results.Any(r => !r.success)))
                    throw new Exception(errorMessage);

                return response;
            }
            catch (Exception ex)
            {
                throw new RestException(url2, requestText, responseText, string.Format("An error occurred while processing a request against '{0}'.", url2), ex);
            }
        }

        private static readonly Func<ServiceArgs, ServiceInfo> GetServiceInfoMemoized = Memoization.Memoize<ServiceArgs, ServiceInfo>(a =>
        {
            var url = string.Format("{0}/layers", Regex.Replace(a.Url, @"/FeatureServer($|/)", a.Url.IsArcGISOnline() ? "/FeatureServer" : "/MapServer", RegexOptions.IgnoreCase));

            var serviceInfo = GetResponse<ServiceInfo>(url, null, a.Credentials, a.Token, a.GdbVersion);

            serviceInfo.AllLayers = (serviceInfo.layers ?? new Layer[] { })
                .Where(l => l.type == "Feature Layer")
                .Concat(serviceInfo.tables ?? new Layer[] { })
                .ToArray();

            var fields = serviceInfo.AllLayers
                .SelectMany(l => l.fields.Where(f => f.domain != null && f.domain.type == "codedValue"))
                .ToArray();

            serviceInfo.AllDomains = fields
                .GroupBy(f => f.domain.name)
                .Select(g => g.First())
                .Select(f =>
                {
                    var convert = GetConvert(f.type);

                    foreach (var c in f.domain.codedValues)
                        c.code = convert(c.code);

                    return f.domain;
                })
                .ToArray();

            var domains = serviceInfo.AllDomains.ToDictionary(d => d.name, d => d);

            foreach (var f in fields)
                f.domain = domains[f.domain.name];

            return serviceInfo;
        });

        private static Func<object, object> GetConvert(string type)
        {
            switch (type)
            {
                case "esriFieldTypeInteger":
                    return o => Convert.ToInt32(o);
                case "esriFieldTypeSmallInteger":
                    return o => Convert.ToInt16(o);
                case "esriFieldTypeDouble":
                    return o => Convert.ToDouble(o);
                case "esriFieldTypeSingle":
                    return o => Convert.ToSingle(o);
                case "esriFieldTypeString":
                    return o => Convert.ToString(o);
                case "esriFieldTypeDate":
                    return o => BaseTime.AddMilliseconds(Convert.ToInt64(o));
                default:
                    return o => o;
            }
        }

        public static ServiceInfo GetServiceInfo(ServiceArgs args)
        {
            return GetServiceInfoMemoized(args);
        }

        private static string CleanWhereClause(string whereClause)
        {
            return HttpUtility.UrlEncode(string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause);
        }

        private static string CleanExtraParameters(string extraParameters)
        {
            return string.IsNullOrWhiteSpace(extraParameters) ? "" : ("&" + extraParameters);
        }

        private static string CleanObjectIds(IEnumerable<int> objectIds)
        {
            return objectIds == null ? "" : HttpUtility.UrlEncode(string.Join(",", objectIds));
        }

        public static OIDSet GetOIDSet(ServiceArgs args, int layerId, string whereClause, string extraParameters)
        {
            var url = string.Format("{0}/{1}/query", args.Url, layerId);
            var data = string.Format("where={0}{1}&returnIdsOnly=true",
                CleanWhereClause(whereClause),
                CleanExtraParameters(extraParameters));

            return GetResponse<OIDSet>(url, data, args.Credentials, args.Token, args.GdbVersion);
        }

        public static FeatureSet GetFeatureSet(ServiceArgs args, int layerId, bool returnGeometry, string whereClause, string extraParameters, IEnumerable<int> objectIds)
        {
            var url = string.Format("{0}/{1}/query", args.Url, layerId);
            var data = string.Format("where={0}{1}&objectIds={2}&returnGeometry={3}&outFields=*",
                CleanWhereClause(whereClause),
                CleanExtraParameters(extraParameters),
                CleanObjectIds(objectIds),
                returnGeometry ? "true" : "false");

            return GetResponse<FeatureSet>(url, data, args.Credentials, args.Token, args.GdbVersion);
        }

        public static bool IsArcGISOnline(this string url)
        {
            return Regex.IsMatch(url, @"\.arcgis\.com/", RegexOptions.IgnoreCase);
        }

        public static TokenInfo GetTokenInfo(string url, string userName, string password)
        {
            var tokenUrl = url.IsArcGISOnline()
                ? "https://www.arcgis.com/sharing/rest/generateToken"
                : string.Format("{0}/tokens/generateToken", Regex.Match(url, @"^http.*?(?=(/rest/services/))", RegexOptions.IgnoreCase).Value);
            var data = string.Format("userName={0}&password={1}&clientid=requestip", userName, password);

            return GetResponse<TokenInfo>(tokenUrl, data, null, null, null);
        }

        public static EditResultSet ApplyEdits(ServiceArgs args, int layerId, string operation, string json)
        {
            var url = string.Format("{0}/{1}/applyEdits", args.Url, layerId);
            var data = string.Format("{0}={1}", operation, HttpUtility.UrlEncode(json));

            return GetResponse<EditResultSet>(url, data, args.Credentials, args.Token, args.GdbVersion);
        }

        public static string GetObjectIdFieldName(this Layer layer)
        {
            var objectIdFields = layer.fields.Where(f => f.type == "esriFieldTypeOID").ToArray();

            if (objectIdFields.Length != 1)
                throw new Exception(string.Format("'{0}' does not have one (and only one) field of type esriFieldTypeOID.", layer.name));

            return objectIdFields.Single().name;
        }

        private static IEnumerable<CodedValue> GetCodeValues(this Layer layer, string domainName)
        {
            var domain = layer.fields.Select(f => f.domain).FirstOrDefault(d => d != null && d.type == "codedValue" && d.name == domainName);

            if (domain == null)
                throw new Exception(string.Format("Coded value domain '{0}' does not exist.", domainName));

            return domain.codedValues;
        }

        public static CodedValue GetCodedValueByCode(this Layer layer, string domainName, object code, bool strict)
        {
            var codedValues = layer.GetCodeValues(domainName).Where(c => c.code.ToString() == code.ToString()).ToArray();

            if (codedValues.Length == 1)
                return codedValues.Single();

            if (codedValues.Length == 0)
            {
                if (strict)
                    throw new Exception(string.Format("Coded value domain '{0}' does not contain code '{1}'.", domainName, code));

                return null;
            }

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
    }

    #region ArcGIS Rest API

    #region Public

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
        /// The geometry type of the layer.
        /// </summary>
        public string geometryType { get; set; }

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

    #endregion

    #region Internal

    internal class Error
    {
    }

    internal class Response
    {
        public Error error { get; set; }
    }

    internal class ServiceInfo : Response
    {
        public Layer[] layers { get; set; }
        public Layer[] tables { get; set; }
        public int? maxRecordCount { get; set; }

        public Layer[] AllLayers { get; set; }
        public Domain[] AllDomains { get; set; }
    }

    internal class TokenInfo : Response
    {
        public string token { get; set; }
        public long expires { get; set; }
    }

    internal class OIDSet : Response
    {
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
        private double _x = double.MinValue;
        private double _y = double.MinValue;
        public double x { get { return _x; } set { _x = value; } }
        public double y { get { return _y; } set { _y = value; } }
        public double[][] points { get; set; }
        public double[][][] paths { get; set; }
        public double[][][] rings { get; set; }
    }

    internal class EditResult
    {
        public int objectId { get; set; }
        public bool success { get; set; }
    }

    #endregion

    #endregion
}
