using System;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides utility functions for geometry objects.
    /// </summary>
    public static class GeometryFunctions
    {
        private static double Length(this double[] p1, double[] p2)
        {
            return Math.Sqrt(Math.Pow(p1[0] - p2[0], 2) + Math.Pow(p1[1] - p2[1], 2));
        }

        /// <summary>
        /// Calculates the length of the polyline.
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double Length(this Polyline polyline)
        {
            if (polyline == null || polyline.paths == null)
                return 0;

            return polyline.paths.SelectMany(p => p.Zip(p.Skip(1), Length)).Sum();
        }

        /// <summary>
        /// Calculates the length of the polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Length(this Polygon polygon)
        {
            if (polygon == null || polygon.rings == null)
                return 0;

            return polygon.rings.SelectMany(r => r.Zip(r.Skip(1), Length)).Sum();
        }

        private static double Area(double[] p1, double[] p2)
        {
            return (-p1[0] + p2[0]) * (p1[1] + p2[1]) / 2;
        }

        /// <summary>
        /// Calculates the area of the polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Area(this Polygon polygon)
        {
            if (polygon == null || polygon.rings == null)
                return 0;

            return polygon.rings.SelectMany(r => r.Zip(r.Skip(1), Area)).Sum();
        }

        /// <summary>
        /// Calculates the distance to the other point.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double Distance(this Point point1, Point point2)
        {
            if (point1 == null || point2 == null)
                throw new Exception("The input geometry is invalid.");

            return Length(new[] { point1.x, point1.y }, new[] { point2.x, point2.y });
        }

        /// <summary>
        /// Calculates the shortest distance to the polyline.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double Distance(this Point point, Polyline polyline)
        {
            if (point == null || polyline == null || polyline.paths == null)
                throw new Exception("The input geometry is invalid.");

            return polyline.paths.SelectMany(p => p.Zip(p.Skip(1), (p1, p2) => Distance(new Vector(p1[0], p1[1]), new Vector(p2[0], p2[1]), point))).Min();
        }

        /// <summary>
        /// Calculates the shortest distance to the polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Distance(this Point point, Polygon polygon)
        {
            if (point == null || polygon == null || polygon.rings == null)
                throw new Exception("The input geometry is invalid.");

            if (point.IsWithin(polygon))
                return 0;

            return polygon.rings.SelectMany(r => r.Zip(r.Skip(1), (p1, p2) => Distance(new Vector(p1[0], p1[1]), new Vector(p2[0], p2[1]), point))).Min();
        }

        private static double Distance(Vector p1, Vector p2, Vector p)
        {
            var d = Math.Pow(Distance(p1, p2), 2);

            if (d == 0)
                return Distance(p, p1);

            var dot = Vector.DotProduct(p - p1, p2 - p1) / d;

            if (dot < 0)
                return Distance(p, p1);

            if (dot > 1)
                return Distance(p, p2);

            return Distance(p, p1 + ((p2 - p1) * dot));
        }

        private static Point Intersect(double[][] l1, double[][] l2)
        {
            if (l1 == null || l2 == null || ReferenceEquals(l1, l2))
                return null;

            var p1 = new Vector(l1[0][0], l1[0][1]);
            var p2 = new Vector(l2[0][0], l2[0][1]);
            var d1 = new Vector(l1[1][0], l1[1][1]) - p1;
            var d2 = new Vector(l2[1][0], l2[1][1]) - p2;

            var d1xd2 = Vector.CrossProduct(d1, d2);

            if (d1xd2 == 0)
                return null;

            var d = p2 - p1;

            var cross1 = Vector.CrossProduct(d, d1 / d1xd2);

            if (cross1 < 0 || cross1 > 1)
                return null;

            var cross2 = Vector.CrossProduct(d, d2 / d1xd2);

            if (cross2 < 0 || cross2 > 1)
                return null;

            return p1 + cross2 * d1;
        }

        private static Point[] Intersect(double[][][] path1, double[][][] path2)
        {
            var lines1 = path1.SelectMany(p => p.Zip(p.Skip(1), (p1, p2) => new[] { p1, p2 })).ToArray();
            var lines2 = path2.SelectMany(p => p.Zip(p.Skip(1), (p1, p2) => new[] { p1, p2 })).ToArray();

            var points = from l1 in lines1
                         from l2 in lines2
                         let p = Intersect(l1, l2)
                         where p != null
                         select p;

            return points.ToArray();
        }

        /// <summary>
        /// Returns intersection points between the two polylines.
        /// </summary>
        /// <param name="polyline1"></param>
        /// <param name="polyline2"></param>
        /// <returns></returns>
        public static Point[] Intersect(this Polyline polyline1, Polyline polyline2)
        {
            return Intersect(polyline1.paths, polyline2.paths);
        }

        /// <summary>
        /// Returns intersection points between the two polyline and the polygon.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static Point[] Intersect(this Polyline polyline, Polygon polygon)
        {
            return Intersect(polyline.paths, polygon.rings);
        }

        /// <summary>
        /// Returns intersection points between the two polygon and the polyline.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static Point[] Intersect(this Polygon polygon, Polyline polyline)
        {
            return Intersect(polygon.rings, polyline.paths);
        }

        /// <summary>
        /// Returns intersection points between the two polygons.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static Point[] Intersect(this Polygon polygon1, Polygon polygon2)
        {
            return Intersect(polygon1.rings, polygon2.rings);
        }

        /// <summary>
        /// Determines if the two polylines intersect at least once.
        /// </summary>
        /// <param name="polyline1"></param>
        /// <param name="polyline2"></param>
        /// <returns></returns>
        public static bool Intersects(this Polyline polyline1, Polyline polyline2)
        {
            return polyline1.Intersect(polyline2).Any();
        }

        /// <summary>
        /// Determines if the polyline intersects the polygon at least once.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool Intersects(this Polyline polyline, Polygon polygon)
        {
            return polyline.Intersect(polygon).Any();
        }

        /// <summary>
        /// Determines if the polygon intersects the polyline at least once.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static bool Intersects(this Polygon polygon, Polyline polyline)
        {
            return polygon.Intersect(polyline).Any();
        }

        /// <summary>
        /// Determines if the two polygons intersect at least once.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static bool Intersects(this Polygon polygon1, Polygon polygon2)
        {
            return polygon1.Intersect(polygon2).Any();
        }

        /// <summary>
        /// Checks if the point is inside of the polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool IsWithin(this Point point, Polygon polygon)
        {
            if (point == null || polygon == null || polygon.rings == null)
                throw new Exception("The input geometry is invalid.");

            return polygon.rings.Where(r => IsWithin(point, r)).Sum(r => r.IsInnerRing() ? -1 : 1) > 0;
        }

        private static bool IsWithin(Point point, double[][] ring)
        {
            return ring
                .Zip(ring.Skip(1), (p1, p2) => new { p1, p2 })
                .Where(o => o.p1[1] > point.y != o.p2[1] > point.y && point.x < (o.p2[0] - o.p1[0]) * (point.y - o.p1[1]) / (o.p2[1] - o.p1[1]) + o.p1[0])
                .Aggregate(false, (isWithin, _) => !isWithin);
        }

        internal static bool IsInnerRing(this double[][] ring)
        {
            return Enumerable.Range(0, ring.Length - 1)
                .Sum(i => ring[i][0] * ring[i + 1][1] - ring[i + 1][0] * ring[i][1]) > 0;
        }
    }
}
