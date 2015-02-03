using System;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting features to text formats.
    /// </summary>
    public static class Text
    {
        /// <summary>
        /// Converts the feature attributes to delimiter-separated values (i.e. CSV).
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <param name="dateFormatter"></param>
        /// <param name="geometrySelector"></param>
        /// <returns></returns>
        public static string ToDelimitedText(this Feature feature, string delimiter = ",", char? qualifier = '"', Func<DateTime, string> dateFormatter = null, Func<Geometry, object> geometrySelector = null)
        {
            if (string.IsNullOrEmpty(delimiter))
                throw new Exception("The delimiter is required.");

            var q = qualifier.ToString();

            if (q != "" && delimiter.Contains(q))
                throw new Exception("The qualifier is not valid.");

            var values = feature.AllFieldNames.Select(n => feature[n]).ToList();

            values.Insert(0, feature.OID);

            if (geometrySelector != null)
                values.Add(geometrySelector(((dynamic)feature).Geometry));

            return string.Join(delimiter, values.Select(o =>
            {
                if (dateFormatter != null && o is DateTime)
                    o = dateFormatter((DateTime)o);

                if (q == "")
                    return o;

                return qualifier + (o ?? "").ToString().Replace(q, q + q) + q;
            }));
        }
    }
}
