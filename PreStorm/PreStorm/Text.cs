using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting features to text formats.
    /// </summary>
    public static class Text
    {
        private static string Join(this IEnumerable<object> values, string delimiter, char? qualifier)
        {
            var d = delimiter ?? "";
            var q = qualifier.ToString();

            if (qualifier != null && d.Contains(q))
                throw new ArgumentException("The qualifier is not valid.", "qualifier");

            return string.Join(d, values.Select(o => q == "" ? o : q + (o ?? "").ToString().Replace(q, q + q) + q));
        }

        private static string ToDelimitedText(this Feature feature, string delimiter, char? qualifier, Func<Geometry, object> geometrySelector, Func<DateTime, string> dateSelector)
        {
            var values = feature.FieldNames.Select(n => feature[n]).ToList();

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

            return values.Select(o => o is DateTime ? dateSelector((DateTime)o) : o).Join(delimiter, qualifier);
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
