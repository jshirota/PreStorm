using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting geometries to well-known text (WKT).
    /// </summary>
    public static class Wkt
    {
        internal static string ToWkt(this Point point)
        {
            if (point == null)
                return null;

            return $"POINT({point.x} {point.y})";
        }

        internal static string ToWkt(this Multipoint multipoint)
        {
            if (multipoint == null)
                return null;

            if (multipoint.points == null || multipoint.points.Length == 0)
                return "MULTIPOINT EMPTY";

            return $"MULTIPOINT({string.Join(",", multipoint.points.Select(p => $"({p[0]} {p[1]})"))})";
        }

        internal static string ToWkt(this Polyline polyline)
        {
            if (polyline == null)
                return null;

            if (polyline.paths == null || polyline.paths.Length == 0)
                return "MULTILINESTRING EMPTY";

            return $"MULTILINESTRING({string.Join(",", polyline.paths.Select(p => $"({string.Join(",", p.Select(c => $"{c[0]} {c[1]}"))})"))})";
        }

        internal static string ToWkt(this Polygon polygon)
        {
            if (polygon == null)
                return null;

            if (polygon.rings == null || polygon.rings.Length == 0)
                return "MULTIPOLYGON EMPTY";

            return $"MULTIPOLYGON({string.Join(",", polygon.GroupRings().Select(p => $"({string.Join(",", p.Select(r => $"({string.Join(",", r.Select(c => $"{c[0]} {c[1]}"))})"))})"))})";
        }

        private static string ToJson(this string wkt, string type)
        {
            if (Regex.IsMatch(wkt, $@"^\s*{type}\s+EMPTY\s*$", RegexOptions.IgnoreCase))
                return null;

            return Regex.Replace(Regex.Replace(wkt, @"(?<x>\-?\d+(\.\d+)?)\s+(?<y>\-?\d+(\.\d+)?)",
                m => $"[{m.Groups["x"]},{m.Groups["y"]}]"), type, "", RegexOptions.IgnoreCase)
                .Replace("(", "[")
                .Replace(")", "]");
        }

        internal static void LoadWkt(this Point point, string wkt)
        {
            var json = wkt.ToJson("POINT");

            if (json == null)
                throw new ArgumentException("Empty point is not supported.", nameof(wkt));

            var coordinates = json.Deserialize<double[][]>()[0];
            point.x = coordinates[0];
            point.y = coordinates[1];
        }

        internal static void LoadWkt(this Multipoint multipoint, string wkt)
        {
            var json = wkt.ToJson("MULTIPOINT");
            multipoint.points = json == null ? new double[][] { } : json.Deserialize<double[][]>();
        }

        internal static void LoadWkt(this Polyline polyline, string wkt)
        {
            var json = wkt.ToJson("MULTILINESTRING");
            polyline.paths = json == null ? new double[][][] { } : json.Deserialize<double[][][]>();
        }

        internal static void LoadWkt(this Polygon polygon, string wkt)
        {
            var json = wkt.ToJson("MULTIPOLYGON");
            polygon.rings = json == null ? new double[][][] { } : json.Deserialize<double[][][][]>().SelectMany(p => p).ToArray();
        }

        /// <summary>
        /// Converts the geometry to well-known text (WKT).
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static string ToWkt(this Geometry geometry)
        {
            if (geometry == null)
                return null;

            var point = geometry as Point;
            if (point != null)
                return point.ToWkt();

            var multipoint = geometry as Multipoint;
            if (multipoint != null)
                return multipoint.ToWkt();

            var polyline = geometry as Polyline;
            if (polyline != null)
                return polyline.ToWkt();

            var polygon = geometry as Polygon;
            if (polygon != null)
                return polygon.ToWkt();

            throw new ArgumentException("This geometry type is not supported.", nameof(geometry));
        }

        /// <summary>
        /// Creates a new geometry from well-known text (WKT).
        /// </summary>
        /// <param name="wkt"></param>
        /// <returns></returns>
        public static Geometry ToGeometry(string wkt)
        {
            if (wkt == null)
                return null;

            var s = wkt.ToUpperInvariant().Trim();

            if (s.StartsWith("POINT"))
                return Point.FromWkt(wkt);
            if (s.StartsWith("MULTIPOINT"))
                return Multipoint.FromWkt(wkt);
            if (s.StartsWith("MULTILINESTRING"))
                return Polyline.FromWkt(wkt);
            if (s.StartsWith("MULTIPOLYGON"))
                return Polygon.FromWkt(wkt);

            throw new ArgumentException("This geometry type is not supported.", nameof(wkt));
        }
    }
}
