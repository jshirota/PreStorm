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

            return polyline.paths.SelectMany(r => r.Zip(r.Skip(1), Length)).Sum();
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
            if (polyline == null || polyline.paths == null)
                return 0;

            return polyline.paths.SelectMany(r => r.Zip(r.Skip(1), (p1, p2) => Distance(new Vector(p1[0], p1[1]), new Vector(p2[0], p2[1]), point))).Min();
        }

        private static double Distance(Vector p1, Vector p2, Vector p)
        {
            var d2 = Math.Pow(Distance(p1, p2), 2);

            if (d2 == 0)
                return Distance(p, p1);

            var t = Vector.DotProduct(p - p1, p2 - p1) / d2;

            if (t < 0)
                return Distance(p, p1);

            if (t > 1)
                return Distance(p, p2);

            return Distance(p, p1 + ((p2 - p1) * t));
        }
    }
}
