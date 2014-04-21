using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PreStorm
{
    /// <summary>
    /// Represents the query parameters as defined in the ArcGIS Rest API.
    /// </summary>
    public class QueryParameters
    {
        private readonly string _queryString;

        /// <summary>
        /// Initializes a new instance of the QueryParameters class.
        /// </summary>
        /// <param name="whereClause">The where clause.  If null, set to "1=1".</param>
        /// <param name="orderByFields">The order by clause i.e. population DESC.</param>
        /// <param name="geometry">The filter geometry.</param>
        /// <param name="geometryType">The type of geometry specified by the geometry parameter.</param>
        /// <param name="spatialRel">The spatial relationship to be applied on the input geometry.</param>
        /// <param name="inSR">The spatial reference of the input geometry.</param>
        /// <param name="outSR">The spatial reference of the output geometry.</param>
        public QueryParameters(string whereClause, string orderByFields = null, string geometry = null, string geometryType = "esriGeometryEnvelope", string spatialRel = "esriSpatialRelIntersects", int? inSR = null, int? outSR = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"where", string.IsNullOrWhiteSpace(whereClause) ? "1=1" : whereClause},
                {"orderByFields", orderByFields},
                {"geometry", geometry},
                {"geometryType", geometryType},
                {"spatialRel", spatialRel},
                {"inSR", inSR},
                {"outSR", outSR},
            };

            _queryString = string.Join("&", parameters
                .Where(o => o.Value != null && !string.IsNullOrWhiteSpace(o.Value.ToString()))
                .Select(o => string.Format("{0}={1}", o.Key, HttpUtility.UrlEncode(o.Value.ToString()))));
        }

        /// <summary>
        /// Returns a QueryParameters object passing the where clause to the constructor.
        /// </summary>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public static implicit operator QueryParameters(string whereClause)
        {
            return new QueryParameters(whereClause);
        }

        public override string ToString()
        {
            return _queryString;
        }
    }

    internal static class QueryParametersExt
    {
        public static string ToQueryString(this QueryParameters queryParameters)
        {
            return queryParameters == null || queryParameters.ToString() == "" ? "where=1%3d1" : queryParameters.ToString();
        }
    }
}
