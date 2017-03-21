using System;
using Newtonsoft.Json;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting geometries to JSON.
    /// </summary>
    public static class Json
    {
        internal static T Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        internal static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static string ToJson(this GeometryBase geometry)
        {
            return geometry?.ToString();
        }

        /// <summary>
        /// Creates a new geometry from JSON.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Geometry ToGeometry(string json)
        {
            if (json == null)
                return null;

            if (json.Contains("x") && json.Contains("y"))
                return Point.FromJson(json);
            if (json.Contains("points"))
                return Multipoint.FromJson(json);
            if (json.Contains("paths"))
                return Polyline.FromJson(json);
            if (json.Contains("rings"))
                return Polygon.FromJson(json);

            throw new ArgumentException("This geometry type is not supported.", nameof(json));
        }
    }
}
