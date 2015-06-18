using System;
using System.Collections;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting features to text formats.
    /// </summary>
    public static class Text
    {
        private static string ToDelimitedText(this Feature feature, string delimiter, char? qualifier, Func<Geometry, object> geometrySelector, Func<DateTime, string> dateSelector)
        {
            if (string.IsNullOrEmpty(delimiter))
                throw new ArgumentException("The delimiter is required.", "delimiter");

            var q = qualifier.ToString();

            if (q != "" && delimiter.Contains(q))
                throw new ArgumentException("The qualifier is not valid.", "qualifier");

            var values = feature.AllFieldNames.Select(n => feature[n]).ToList();

            values.Insert(0, feature.OID);

            if (geometrySelector != null)
            {
                var o = geometrySelector(((dynamic)feature).Geometry);

                if (o is string)
                    values.Add(o);
                else
                    values.AddRange((o as IEnumerable ?? new[] { o }).Cast<object>());
            }

            dateSelector = dateSelector ?? (d => d.ToString("o"));

            return string.Join(delimiter, values.Select(o =>
            {
                if (o is DateTime)
                    o = dateSelector((DateTime)o);

                if (q == "")
                    return o;

                return qualifier + (o ?? "").ToString().Replace(q, q + q) + q;
            }));
        }

        /// <summary>
        /// Converts the feature attributes to delimiter-separated values (i.e. CSV).
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <param name="dateSelector"></param>
        /// <returns></returns>
        public static string ToText(this Feature feature, string delimiter = ",", char? qualifier = '"', Func<DateTime, string> dateSelector = null)
        {
            return feature.ToDelimitedText(delimiter, qualifier, null, dateSelector);
        }

        /// <summary>
        /// Converts the feature attributes to delimiter-separated values (i.e. CSV).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <param name="geometrySelector"></param>
        /// <param name="dateSelector"></param>
        /// <returns></returns>
        public static string ToText<T>(this Feature<T> feature, string delimiter = ",", char? qualifier = '"', Func<T, object> geometrySelector = null, Func<DateTime, string> dateSelector = null) where T : Geometry
        {
            return feature.ToDelimitedText(delimiter, qualifier, geometrySelector == null ? (Func<Geometry, object>)null : g => geometrySelector((T)g), dateSelector);
        }
    }
}
