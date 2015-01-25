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
            var q = qualifier.ToString();

            if (q == delimiter)
                throw new Exception("The qualifier cannot be same as the delimiter.");

            var values = feature.GetType().GetMappings().Select(m => m.Property.GetValue(feature, null)).ToList();

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
