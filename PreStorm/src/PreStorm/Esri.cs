using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PreStorm
{
    internal static class Esri
    {
        public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static T GetResponse<T>(string url, string data, ICredentials credentials, Token token, string gdbVersion) where T : Response
        {
            var parameters = new Dictionary<string, object> { { "token", token }, { "gdbVersion", gdbVersion }, { "f", "json" } };
            var queryString = string.Join("&", parameters.Where(p => p.Value != null).Select(p => $"{p.Key}={Compatibility.UrlEncode(p.Value.ToString())}"));

            var isPost = data != null;

            var url2 = isPost ? url : (url + "?" + queryString);
            var requestText = isPost ? data + "&" + queryString : "";

            string responseText = null;

            try
            {
                var requestModifier = credentials == null ? (Action<HttpWebRequest>)null : r => r.Credentials = credentials;
                responseText = isPost ? Http.Post(url2, requestText, requestModifier) : Http.Get(url2, requestModifier);

                var response = responseText.Deserialize<T>();
                var errorMessage = "ArcGIS Server returned an error response.";

                if (response.error != null)
                    throw new InvalidOperationException(errorMessage);

                var resultSet = response as EditResultSet;

                if (resultSet != null && new[] { resultSet.addResults, resultSet.updateResults, resultSet.deleteResults }.Any(results => results == null || results.Any(r => !r.success)))
                    throw new InvalidOperationException(errorMessage);

                return response;
            }
            catch (Exception ex)
            {
                throw new RestException(url2, requestText, responseText, $"An error occurred while processing a request against '{url2}'.", ex);
            }
        }

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
            var url = $"{Regex.Replace(args.Url, @"/FeatureServer($|/)", args.Url.IsArcGISOnline() ? "/FeatureServer" : "/MapServer", RegexOptions.IgnoreCase)}/layers";

            var serviceInfo = GetResponse<ServiceInfo>(url, null, args.Credentials, args.Token, args.GdbVersion);

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
        }

        private static string CleanWhereClause(string whereClause)
        {
            return Compatibility.UrlEncode(string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause);
        }

        private static string CleanExtraParameters(string extraParameters)
        {
            return string.IsNullOrWhiteSpace(extraParameters) ? "" : ("&" + extraParameters);
        }

        private static string CleanObjectIds(IEnumerable<int> objectIds)
        {
            return objectIds == null ? "" : Compatibility.UrlEncode(string.Join(",", objectIds));
        }

        public static OIDSet GetOIDSet(ServiceArgs args, int layerId, string whereClause, string extraParameters)
        {
            var url = $"{args.Url}/{layerId}/query";
            var data = $"where={CleanWhereClause(whereClause)}{CleanExtraParameters(extraParameters)}&returnIdsOnly=true";

            return GetResponse<OIDSet>(url, data, args.Credentials, args.Token, args.GdbVersion);
        }

        public static FeatureSet GetFeatureSet(ServiceArgs args, int layerId, bool returnGeometry, bool returnZ, string whereClause, string extraParameters, IEnumerable<int> objectIds)
        {
            var url = $"{args.Url}/{layerId}/query";
            var data = $"where={CleanWhereClause(whereClause)}{CleanExtraParameters(extraParameters)}&objectIds={CleanObjectIds(objectIds)}&returnGeometry={(returnGeometry ? "true" : "false")}&returnZ={(returnZ ? "true" : "false")}&outFields=*";

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
                : $"{Regex.Match(url, @"^http.*?(?=(/rest/services/))", RegexOptions.IgnoreCase).Value}/tokens/generateToken";
            var data = $"userName={userName}&password={password}&clientid=requestip";

            return GetResponse<TokenInfo>(tokenUrl, data, null, null, null);
        }

        public static EditResultSet ApplyEdits(ServiceArgs args, int layerId, string operation, string json)
        {
            var url = $"{args.Url}/{layerId}/applyEdits";
            var data = $"{operation}={Compatibility.UrlEncode(operation == "deletes" ? json : RemoveNullZ(json))}";

            return GetResponse<EditResultSet>(url, data, args.Credentials, args.Token, args.GdbVersion);
        }

        private static string RemoveNullZ(string json)
        {
            var array = json.Deserialize<object>() as object[];

            if (array == null)
                return json;

            foreach (var d in array.OfType<Dictionary<string, object>>())
            {
                if (d.ContainsKey("geometry"))
                {
                    var g = d["geometry"] as Dictionary<string, object>;

                    if (g != null && g.ContainsKey("z") && g["z"] == null && g["x"] != null && g["y"] != null)
                        g.Remove("z");
                }
            }

            return array.Serialize();
        }

        public static string GetObjectIdFieldName(this Layer layer)
        {
            var objectIdFields = layer.fields.Where(f => f.type == "esriFieldTypeOID").ToArray();

            if (objectIdFields.Length != 1)
                throw new InvalidOperationException($"'{layer.name}' does not have one (and only one) field of type esriFieldTypeOID.");

            return objectIdFields.Single().name;
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

        /// <summary>
        /// Indicates if the layer supports the Z coordinates.
        /// </summary>
        public bool hasZ { get; set; }
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
        /// The length of the field.
        /// </summary>
        public int? length { get; set; }

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
        public double x { get; set; } = double.MinValue;
        public double y { get; set; } = double.MinValue;
        public double? z { get; set; }
        public double[][] points { get; set; }
        public double[][][] paths { get; set; }
        public object[][] curvePaths { get; set; }
        public double[][][] rings { get; set; }
        public object[][] curveRings { get; set; }
    }

    internal class EditResult
    {
        public int objectId { get; set; }
        public bool success { get; set; }
    }

    #endregion

    #endregion
}
