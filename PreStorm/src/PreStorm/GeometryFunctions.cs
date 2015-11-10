using System;
using System.Collections.Generic;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides utility functions for geometry objects.
    /// </summary>
    public static class GeometryFunctions
    {
        #region Helpers

        private static bool Null(params object[] geometries)
        {
            return geometries != null && geometries.Any(g => g == null);
        }

        private static void AssertNotNull(params object[] geometries)
        {
            if (Null(geometries))
                throw new ArgumentException("Input geometries cannot be null.", nameof(geometries));
        }

        private static double Length(this double[] p1, double[] p2)
        {
            return Math.Sqrt(Math.Pow(p1[0] - p2[0], 2) + Math.Pow(p1[1] - p2[1], 2));
        }

        private static double Area(double[] p1, double[] p2)
        {
            return (-p1[0] + p2[0]) * (p1[1] + p2[1]) / 2;
        }

        private static double Distance(this Point point, double[][][] paths)
        {
            return paths.SelectMany(p => p.Zip(p.Skip(1), (p1, p2) => Distance(new Vector(p1[0], p1[1]), new Vector(p2[0], p2[1]), point))).Min();
        }

        private static double Distance(this double[][][] paths1, double[][][] paths2)
        {
            return paths1.SelectMany(path => path.Select(p => new Point(p[0], p[1]).Distance(paths2))).Min();
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

        private static Envelope Extent(this double[][] points)
        {
            return new Envelope
            {
                xmin = points.Min(p => p[0]),
                ymin = points.Min(p => p[1]),
                xmax = points.Max(p => p[0]),
                ymax = points.Max(p => p[1])
            };
        }

        private static Envelope Extent(this double[][][] paths)
        {
            return paths.SelectMany(p => p).ToArray().Extent();
        }

        private static Point Intersect(double[][] l1, double[][] l2)
        {
            if (l1 == null || l2 == null || l1.Equals(l2))
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

        private static bool Intersects(this Envelope extent1, Envelope extent2)
        {
            return extent1.xmin <= extent2.xmax
                && extent1.ymin <= extent2.ymax
                && extent1.xmax >= extent2.xmin
                && extent1.ymax >= extent2.ymin;
        }

        private static bool Contains(double[][] ring, Point point)
        {
            return ring
                .Zip(ring.Skip(1), (p1, p2) => new { p1, p2 })
                .Where(o => o.p1[1] > point.y != o.p2[1] > point.y && point.x < (o.p2[0] - o.p1[0]) * (point.y - o.p1[1]) / (o.p2[1] - o.p1[1]) + o.p1[0])
                .Aggregate(false, (isWithin, _) => !isWithin);
        }

        private static bool Contains(this Polygon polygon, double[][] points)
        {
            return points.All(p => polygon.Contains(new Point(p[0], p[1])));
        }

        internal static bool IsInnerRing(this double[][] ring)
        {
            return Enumerable.Range(0, ring.Length - 1)
                .Sum(i => ring[i][0] * ring[i + 1][1] - ring[i + 1][0] * ring[i][1]) > 0;
        }

        internal static List<List<double[][]>> GroupRings(this Polygon polygon)
        {
            var polygons = new List<List<double[][]>>();

            foreach (var ring in polygon.rings)
            {
                var isInnerRing = ring.IsInnerRing();

                if (!isInnerRing)
                    polygons.Add(new List<double[][]>());

                if (polygons.Count == 0)
                    throw new InvalidOperationException("The first ring of a polygon must be an outer ring.");

                polygons.Last().Add(ring);
            }

            return polygons;
        }

        #endregion

        #region JSON

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static string ToJson(this GeometryBase geometry)
        {
            return geometry?.ToString();
        }

        #endregion

        #region Length / Area

        /// <summary>
        /// Calculates the length of the polyline.
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double Length(this Polyline polyline)
        {
            if (polyline?.paths == null)
                return 0;

            return polyline.paths.SelectMany(p => p.Zip(p.Skip(1), Length)).Sum();
        }

        /// <summary>
        /// Calculates the perimeter of the polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Perimeter(this Polygon polygon)
        {
            if (polygon?.rings == null)
                return 0;

            return polygon.rings.SelectMany(r => r.Zip(r.Skip(1), Length)).Sum();
        }

        /// <summary>
        /// Calculates the area of the polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Area(this Polygon polygon)
        {
            if (polygon?.rings == null)
                return 0;

            return polygon.rings.SelectMany(r => r.Zip(r.Skip(1), Area)).Sum();
        }

        #endregion

        #region Distance

        /// <summary>
        /// Calculates the distance to the other point.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double Distance(this Point point1, Point point2)
        {
            AssertNotNull(point1, point2);

            if (point1.Equals(point2))
                return 0;

            return Length(new[] { point1.x, point1.y }, new[] { point2.x, point2.y });
        }

        /// <summary>
        /// Calculates the shortest distance to the multipoint.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static double Distance(this Point point, Multipoint multipoint)
        {
            AssertNotNull(point, multipoint);

            return multipoint.points.Min(p => Length(new[] { point.x, point.y }, p));
        }

        /// <summary>
        /// Calculates the shortest distance to the polyline.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double Distance(this Point point, Polyline polyline)
        {
            AssertNotNull(point, polyline);

            return point.Distance(polyline.paths);
        }

        /// <summary>
        /// Calculates the shortest distance to the polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Distance(this Point point, Polygon polygon)
        {
            AssertNotNull(point, polygon);

            if (polygon.Contains(point))
                return 0;

            return point.Distance(polygon.rings);
        }

        /// <summary>
        /// Calculates the shortest distance to the point.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double Distance(this Multipoint multipoint, Point point)
        {
            AssertNotNull(multipoint, point);

            return point.Distance(multipoint);
        }

        /// <summary>
        /// Calculates the distance to the other multipoint.
        /// </summary>
        /// <param name="multipoint1"></param>
        /// <param name="multipoint2"></param>
        /// <returns></returns>
        public static double Distance(this Multipoint multipoint1, Multipoint multipoint2)
        {
            AssertNotNull(multipoint1, multipoint2);

            if (multipoint1.Equals(multipoint2))
                return 0;

            return multipoint1.points.SelectMany(p1 => multipoint2.points.Select(p2 => Length(p1, p2))).Min();
        }

        /// <summary>
        /// Calculates the shortest distance to the polyline.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double Distance(this Multipoint multipoint, Polyline polyline)
        {
            AssertNotNull(multipoint, polyline);

            return multipoint.points.Min(p => new Point(p[0], p[1]).Distance(polyline));
        }

        /// <summary>
        /// Calculates the shortest distance to the polygon.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Distance(this Multipoint multipoint, Polygon polygon)
        {
            AssertNotNull(multipoint, polygon);

            return multipoint.points.Min(p => new Point(p[0], p[1]).Distance(polygon));
        }

        /// <summary>
        /// Calculates the shortest distance to the point.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double Distance(this Polyline polyline, Point point)
        {
            AssertNotNull(polyline, point);

            return point.Distance(polyline);
        }

        /// <summary>
        /// Calculates the shortest distance to the multipoint.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static double Distance(this Polyline polyline, Multipoint multipoint)
        {
            AssertNotNull(polyline, multipoint);

            return multipoint.Distance(polyline);
        }

        /// <summary>
        /// Calculates the distance to the other polyline.
        /// </summary>
        /// <param name="polyline1"></param>
        /// <param name="polyline2"></param>
        /// <returns></returns>
        public static double Distance(this Polyline polyline1, Polyline polyline2)
        {
            AssertNotNull(polyline1, polyline2);

            if (polyline1.Equals(polyline2))
                return 0;

            if (polyline1.Intersects(polyline2))
                return 0;

            return Math.Min(polyline1.paths.Distance(polyline2.paths), polyline2.paths.Distance(polyline1.paths));
        }

        /// <summary>
        /// Calculates the shortest distance to the polygon.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static double Distance(this Polyline polyline, Polygon polygon)
        {
            AssertNotNull(polyline, polygon);

            if (polyline.Intersects(polygon))
                return 0;

            if (polyline.Within(polygon))
                return 0;

            return Math.Min(polyline.paths.Distance(polygon.rings), polygon.rings.Distance(polyline.paths));
        }

        /// <summary>
        /// Calculates the shortest distance to the point.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double Distance(this Polygon polygon, Point point)
        {
            AssertNotNull(polygon, point);

            return point.Distance(polygon);
        }

        /// <summary>
        /// Calculates the shortest distance to the multipoint.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static double Distance(this Polygon polygon, Multipoint multipoint)
        {
            AssertNotNull(polygon, multipoint);

            return multipoint.Distance(polygon);
        }

        /// <summary>
        /// Calculates the shortest distance to the polyline.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static double Distance(this Polygon polygon, Polyline polyline)
        {
            AssertNotNull(polygon, polyline);

            return polyline.Distance(polygon);
        }

        /// <summary>
        /// Calculates the distance to the other polygon.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static double Distance(this Polygon polygon1, Polygon polygon2)
        {
            AssertNotNull(polygon1, polygon2);

            if (polygon1.Equals(polygon2))
                return 0;

            if (polygon1.Intersects(polygon2))
                return 0;

            if (polygon1.Within(polygon2))
                return 0;

            if (polygon2.Within(polygon1))
                return 0;

            return Math.Min(polygon1.rings.Distance(polygon2.rings), polygon2.rings.Distance(polygon1.rings));
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Point> feature1, Feature<Point> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Point> feature1, Feature<Multipoint> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Point> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Point> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Multipoint> feature1, Feature<Point> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Multipoint> feature1, Feature<Multipoint> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Multipoint> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Multipoint> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polyline> feature1, Feature<Point> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polyline> feature1, Feature<Multipoint> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polyline> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polyline> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polygon> feature1, Feature<Point> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polygon> feature1, Feature<Multipoint> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polygon> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        /// <summary>
        /// Calculates the distance between two features.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static double Distance(this Feature<Polygon> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Distance(feature2.Geometry);
        }

        #endregion

        #region Extent

        /// <summary>
        /// Returns the extent of this point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Envelope Extent(this Point point)
        {
            AssertNotNull(point);

            var extent = new Envelope
            {
                xmin = point.x,
                ymin = point.y,
                xmax = point.x,
                ymax = point.y,
                spatialReference = point.spatialReference
            };

            return extent;
        }

        /// <summary>
        /// Returns the extent of this multipoint.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static Envelope Extent(this Multipoint multipoint)
        {
            AssertNotNull(multipoint);

            var extent = multipoint.points.Extent();
            extent.spatialReference = multipoint.spatialReference;

            return extent;
        }

        /// <summary>
        /// Returns the extent of this polyline.
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static Envelope Extent(this Polyline polyline)
        {
            AssertNotNull(polyline);

            var extent = polyline.paths.Extent();
            extent.spatialReference = polyline.spatialReference;

            return extent;
        }

        /// <summary>
        /// Returns the extent of this polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static Envelope Extent(this Polygon polygon)
        {
            AssertNotNull(polygon);

            var extent = polygon.rings.Extent();
            extent.spatialReference = polygon.spatialReference;

            return extent;
        }

        /// <summary>
        /// Buffers the extent by the specified distance.
        /// </summary>
        /// <param name="extent"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Envelope Buffer(this Envelope extent, double distance)
        {
            AssertNotNull(extent);

            var xmin = extent.xmin - distance;
            var ymin = extent.ymin - distance;
            var xmax = extent.xmax + distance;
            var ymax = extent.ymax + distance;

            if (xmin > xmax)
                xmin = xmax = (xmin + xmax) / 2;

            if (ymin > ymax)
                ymin = ymax = (ymin + ymax) / 2;

            return new Envelope
            {
                xmin = xmin,
                ymin = ymin,
                xmax = xmax,
                ymax = ymax,
                spatialReference = extent.spatialReference
            };
        }

        /// <summary>
        /// Returns the centre point of the extent.
        /// </summary>
        /// <param name="extent"></param>
        /// <returns></returns>
        public static Point Centre(this Envelope extent)
        {
            AssertNotNull(extent);

            return new Point
            {
                x = (extent.xmin + extent.xmax) / 2,
                y = (extent.ymin + extent.ymax) / 2,
                spatialReference = extent.spatialReference
            };
        }

        #endregion

        #region Intersect / Intersects

        /// <summary>
        /// Returns intersection points between the two polylines.
        /// </summary>
        /// <param name="polyline1"></param>
        /// <param name="polyline2"></param>
        /// <returns></returns>
        public static Point[] Intersect(this Polyline polyline1, Polyline polyline2)
        {
            if (Null(polyline1, polyline2))
                return new Point[] { };

            return !polyline1.Extent().Intersects(polyline2.Extent())
                ? new Point[] { }
                : Intersect(polyline1.paths, polyline2.paths);
        }

        /// <summary>
        /// Determines if the polyline intersects the other polyline.
        /// </summary>
        /// <param name="polyline1"></param>
        /// <param name="polyline2"></param>
        /// <returns></returns>
        public static bool Intersects(this Polyline polyline1, Polyline polyline2)
        {
            return polyline1.Intersect(polyline2).Any();
        }

        /// <summary>
        /// Determines if the polyline intersects the polygon.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool Intersects(this Polyline polyline, Polygon polygon)
        {
            return polyline.Intersects(new Polyline { paths = polygon.rings });
        }

        /// <summary>
        /// Determines if the polygon intersects the polyline.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static bool Intersects(this Polygon polygon, Polyline polyline)
        {
            return new Polyline { paths = polygon.rings }.Intersects(polyline);
        }

        /// <summary>
        /// Determines if the polygon intersects the other polygon.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static bool Intersects(this Polygon polygon1, Polygon polygon2)
        {
            return new Polyline { paths = polygon1.rings }.Intersects(new Polyline { paths = polygon2.rings });
        }

        /// <summary>
        /// Returns intersection points between the two polylines.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static Point[] Intersect(this Feature<Polyline> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Intersect(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polyline intersects the other polyline.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Intersects(this Feature<Polyline> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Intersects(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polyline intersects the polygon.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Intersects(this Feature<Polyline> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Intersects(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polygon intersects the polyline.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Intersects(this Feature<Polygon> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Intersects(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polyline intersects the other polyline.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Intersects(this Feature<Polygon> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Intersects(feature2.Geometry);
        }

        #endregion

        #region Contains / Within

        /// <summary>
        /// Determines if the polygon contains the point.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool Contains(this Polygon polygon, Point point)
        {
            if (Null(polygon, point))
                return false;

            return polygon.rings.Where(r => Contains(r, point)).Sum(r => r.IsInnerRing() ? -1 : 1) > 0;
        }

        /// <summary>
        /// Determines if the polygon completely contains the multipoint.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static bool Contains(this Polygon polygon, Multipoint multipoint)
        {
            if (Null(polygon, multipoint))
                return false;

            return polygon.Contains(multipoint.points);
        }

        /// <summary>
        /// Determines if the polygon completely contains the polyline.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static bool Contains(this Polygon polygon, Polyline polyline)
        {
            if (Null(polygon, polyline))
                return false;

            return !polygon.Intersects(polyline) && polyline.paths.All(polygon.Contains);
        }

        /// <summary>
        /// Determines if the polygon completely contains the other polygon.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static bool Contains(this Polygon polygon1, Polygon polygon2)
        {
            if (Null(polygon1, polygon2))
                return false;

            return !polygon1.Intersects(polygon2) && polygon2.rings.All(polygon1.Contains);
        }

        /// <summary>
        /// Determines if the polygon contains the point.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Contains(this Feature<Polygon> feature1, Feature<Point> feature2)
        {
            return feature1.Geometry.Contains(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polygon completely contains the multipoint.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Contains(this Feature<Polygon> feature1, Feature<Multipoint> feature2)
        {
            return feature1.Geometry.Contains(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polygon completely contains the polyline.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Contains(this Feature<Polygon> feature1, Feature<Polyline> feature2)
        {
            return feature1.Geometry.Contains(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polygon completely contains the other polygon.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Contains(this Feature<Polygon> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Contains(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the point is inside the polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool Within(this Point point, Polygon polygon)
        {
            if (Null(point, polygon))
                return false;

            return polygon.Contains(point);
        }

        /// <summary>
        /// Determines if the multipoint is inside the polygon.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool Within(this Multipoint multipoint, Polygon polygon)
        {
            if (Null(multipoint, polygon))
                return false;

            return polygon.Contains(multipoint);
        }

        /// <summary>
        /// Determines if the polyline is inside the polygon.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static bool Within(this Polyline polyline, Polygon polygon)
        {
            if (Null(polyline, polygon))
                return false;

            return polygon.Contains(polyline);
        }

        /// <summary>
        /// Determines if the polygon is inside the other polygon.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <returns></returns>
        public static bool Within(this Polygon polygon1, Polygon polygon2)
        {
            if (Null(polygon1, polygon2))
                return false;

            return polygon2.Contains(polygon1);
        }

        /// <summary>
        /// Determines if the point is inside the polygon.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Within(this Feature<Point> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Within(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the multipoint is inside the polygon.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Within(this Feature<Multipoint> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Within(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polyline is inside the polygon.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Within(this Feature<Polyline> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Within(feature2.Geometry);
        }

        /// <summary>
        /// Determines if the polygon is inside the other polygon.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <returns></returns>
        public static bool Within(this Feature<Polygon> feature1, Feature<Polygon> feature2)
        {
            return feature1.Geometry.Within(feature2.Geometry);
        }

        #endregion

        #region WithinDistance

        /// <summary>
        /// Determines if the point is within the specified distance from the other point.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Point point1, Point point2, double distance)
        {
            if (Null(point1, point2))
                return false;

            return point1.Distance(point2) < distance;
        }

        /// <summary>
        /// Determines if the point is within the specified distance from the multipoint.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="multipoint"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Point point, Multipoint multipoint, double distance)
        {
            if (Null(point, multipoint))
                return false;

            return point.Extent().Buffer(distance).Intersects(multipoint.Extent()) && point.Distance(multipoint) < distance;
        }

        /// <summary>
        /// Determines if the point is within the specified distance from the polyline.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polyline"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Point point, Polyline polyline, double distance)
        {
            if (Null(point, polyline))
                return false;

            return point.Extent().Buffer(distance).Intersects(polyline.Extent()) && point.Distance(polyline) < distance;
        }

        /// <summary>
        /// Determines if the point is within the specified distance from the polygon.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Point point, Polygon polygon, double distance)
        {
            if (Null(point, polygon))
                return false;

            return point.Extent().Buffer(distance).Intersects(polygon.Extent()) && point.Distance(polygon) < distance;
        }

        /// <summary>
        /// Determines if the multipoint is within the specified distance from the point.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Multipoint multipoint, Point point, double distance)
        {
            if (Null(multipoint, point))
                return false;

            return point.Distance(multipoint) < distance;
        }

        /// <summary>
        /// Determines if the multipoint is within the specified distance from the other multipoint.
        /// </summary>
        /// <param name="multipoint1"></param>
        /// <param name="multipoint2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Multipoint multipoint1, Multipoint multipoint2, double distance)
        {
            if (Null(multipoint1, multipoint2))
                return false;

            return multipoint1.Extent().Buffer(distance).Intersects(multipoint2.Extent()) && multipoint1.Distance(multipoint2) < distance;
        }

        /// <summary>
        /// Determines if the multipoint is within the specified distance from the polyline.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="polyline"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Multipoint multipoint, Polyline polyline, double distance)
        {
            if (Null(multipoint, polyline))
                return false;

            return multipoint.Extent().Buffer(distance).Intersects(polyline.Extent()) && multipoint.Distance(polyline) < distance;
        }

        /// <summary>
        /// Determines if the multipoint is within the specified distance from the polygon.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <param name="polygon"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Multipoint multipoint, Polygon polygon, double distance)
        {
            if (Null(multipoint, polygon))
                return false;

            return multipoint.Extent().Buffer(distance).Intersects(polygon.Extent()) && multipoint.Distance(polygon) < distance;
        }

        /// <summary>
        /// Determines if the polyline is within the specified distance from the point.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polyline polyline, Point point, double distance)
        {
            if (Null(polyline, point))
                return false;

            return point.Distance(polyline) < distance;
        }

        /// <summary>
        /// Determines if the polyline is within the specified distance from the multipoint.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="multipoint"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polyline polyline, Multipoint multipoint, double distance)
        {
            if (Null(polyline, multipoint))
                return false;

            return multipoint.Distance(polyline) < distance;
        }

        /// <summary>
        /// Determines if the polyline is within the specified distance from the other polyline.
        /// </summary>
        /// <param name="polyline1"></param>
        /// <param name="polyline2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polyline polyline1, Polyline polyline2, double distance)
        {
            if (Null(polyline1, polyline2))
                return false;

            return polyline1.Extent().Buffer(distance).Intersects(polyline2.Extent()) && polyline1.Distance(polyline2) < distance;
        }

        /// <summary>
        /// Determines if the polyline is within the specified distance from the polygon.
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="polygon"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polyline polyline, Polygon polygon, double distance)
        {
            if (Null(polyline, polygon))
                return false;

            return polyline.Extent().Buffer(distance).Intersects(polygon.Extent()) && polyline.Distance(polygon) < distance;
        }

        /// <summary>
        /// Determines if the polygon is within the specified distance from the point.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polygon polygon, Point point, double distance)
        {
            if (Null(polygon, point))
                return false;

            return point.Distance(polygon) < distance;
        }

        /// <summary>
        /// Determines if the polygon is within the specified distance from the multipoint.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="multipoint"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polygon polygon, Multipoint multipoint, double distance)
        {
            if (Null(polygon, multipoint))
                return false;

            return multipoint.Distance(polygon) < distance;
        }

        /// <summary>
        /// Determines if the polygon is within the specified distance from the polyline.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="polyline"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polygon polygon, Polyline polyline, double distance)
        {
            if (Null(polygon, polyline))
                return false;

            return polyline.Distance(polygon) < distance;
        }

        /// <summary>
        /// Determines if the polygon is within the specified distance from the other polygon.
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Polygon polygon1, Polygon polygon2, double distance)
        {
            if (Null(polygon1, polygon2))
                return false;

            return polygon1.Extent().Buffer(distance).Intersects(polygon2.Extent()) && polygon1.Distance(polygon2) < distance;
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Point> feature1, Feature<Point> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Point> feature1, Feature<Multipoint> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Point> feature1, Feature<Polyline> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Point> feature1, Feature<Polygon> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Multipoint> feature1, Feature<Point> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Multipoint> feature1, Feature<Multipoint> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Multipoint> feature1, Feature<Polyline> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Multipoint> feature1, Feature<Polygon> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polyline> feature1, Feature<Point> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polyline> feature1, Feature<Multipoint> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polyline> feature1, Feature<Polyline> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polyline> feature1, Feature<Polygon> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polygon> feature1, Feature<Point> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polygon> feature1, Feature<Multipoint> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polygon> feature1, Feature<Polyline> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        /// <summary>
        /// Determines if the feature is within the specified distance from the other feature.
        /// </summary>
        /// <param name="feature1"></param>
        /// <param name="feature2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool WithinDistance(this Feature<Polygon> feature1, Feature<Polygon> feature2, double distance)
        {
            return feature1.Geometry.WithinDistance(feature2.Geometry, distance);
        }

        #endregion
    }
}
