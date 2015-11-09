using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting geometries to well-known text (WKT).
    /// </summary>
    public static class Wkt
    {
        /// <summary>
        /// Converts the geometry to well-known text (WKT).
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static string ToWkt(this Point point)
        {
            return $"POINT({point.x} {point.y})";
        }

        /// <summary>
        /// Converts the geometry to well-known text (WKT).
        /// </summary>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static string ToWkt(this Multipoint multipoint)
        {
            return $"MULTIPOINT({string.Join(",", multipoint.points.Select(p => $"({p[0]} {p[1]})"))})";
        }

        /// <summary>
        /// Converts the geometry to well-known text (WKT).
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static string ToWkt(this Polyline polyline)
        {
            return $"MULTILINESTRING({string.Join(",", polyline.paths.Select(p => $"({string.Join(",", p.Select(c => $"{c[0]} {c[1]}"))})"))})";
        }

        /// <summary>
        /// Converts the geometry to well-known text (WKT).
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static string ToWkt(this Polygon polygon)
        {
            return $"MULTIPOLYGON({string.Join(",", polygon.GroupRings().Select(p => $"({string.Join(",", p.Select(r => $"({string.Join(",", r.Select(c => $"{c[0]} {c[1]}"))})"))})"))})";
        }
    }
}
