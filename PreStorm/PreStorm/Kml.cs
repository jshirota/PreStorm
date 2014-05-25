using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PreStorm
{
    /// <summary>
    /// Provides extension methods for converting geometries and features to KML.
    /// </summary>
    public static class Kml
    {
        private static readonly XNamespace kml = "http://www.opengis.net/kml/2.2";

        private static string ToCoordinates(this Point geometry, double z)
        {
            return geometry.x + "," + geometry.y + "," + z;
        }

        private static string ToCoordinates(this double[] coordinates, double z)
        {
            return coordinates[0] + "," + coordinates[1] + "," + z;
        }

        private static string ToCoordinates(this double[][] coordinates, double z)
        {
            return string.Join(" ", coordinates.Select(c => c.ToCoordinates(z)));
        }

        private static bool IsInnerRing(double[][] ring)
        {
            return Enumerable.Range(0, ring.Length - 1)
                .Sum(i => ring[i][0] * ring[i + 1][1] - ring[i + 1][0] * ring[i][1]) > 0;
        }

        private static XElement ToKmlPoint(this Point geometry, double z, XElement[] extraElements)
        {
            return new XElement(kml + "Point", extraElements,
                new XElement(kml + "coordinates", geometry.ToCoordinates(z)));
        }

        private static XElement ToKmlMultipoint(this Multipoint geometry, double z, XElement[] extraElements)
        {
            return new XElement(kml + "MultiGeometry",
                geometry.points.Select(p => new Point { x = p[0], y = p[1] }.ToKmlPoint(z, extraElements)));
        }

        private static XElement ToKmlPolyline(this Polyline geometry, double z, XElement[] extraElements)
        {
            return new XElement(kml + "MultiGeometry",
                geometry.paths.Select(p =>
                    new XElement(kml + "LineString", extraElements,
                        new XElement(kml + "coordinates", p.ToCoordinates(z)))));
        }

        private static XElement ToKmlPolygon(this Polygon geometry, double z, XElement[] extraElements)
        {
            var polygons = new List<XElement>();

            foreach (var ring in geometry.rings)
            {
                var isInnerRing = IsInnerRing(ring);

                if (!isInnerRing)
                    polygons.Add(new XElement(kml + "Polygon", extraElements));

                polygons.Last().Add(new XElement(kml + (isInnerRing ? "innerBoundaryIs" : "outerBoundaryIs"),
                    new XElement(kml + "LinearRing",
                        new XElement(kml + "coordinates", ring.ToCoordinates(z)))));
            }

            return new XElement(kml + "MultiGeometry", polygons);
        }

        private static XElement[] GetElements<T>(this T feature, Dictionary<string, Func<T, object>> elements) where T : Feature
        {
            return elements.Where(e => e.Value != null).Select(e => new XElement(kml + e.Key, e.Value(feature))).ToArray();
        }

        /// <summary>
        /// Converts the geometry to KML.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="z">The altitude in meters.</param>
        /// <param name="geometryElements">Any extra geometry elements (i.e. altitudeMode).</param>
        /// <returns></returns>
        public static XElement ToKml(this Geometry geometry, double z = 0, XElement[] geometryElements = null)
        {
            if (geometry == null)
                return null;

            var point = geometry as Point;
            if (point != null)
                return point.ToKmlPoint(z, geometryElements);

            var multipoint = geometry as Multipoint;
            if (multipoint != null)
                return multipoint.ToKmlMultipoint(z, geometryElements);

            var polyline = geometry as Polyline;
            if (polyline != null)
                return polyline.ToKmlPolyline(z, geometryElements);

            var polygon = geometry as Polygon;
            if (polygon != null)
                return polygon.ToKmlPolygon(z, geometryElements);

            throw new Exception("This geometry type is not supported.");
        }

        /// <summary>
        /// Converts the feature to KML.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="name">The name for the placemark.</param>
        /// <param name="z">The altitude in meters.</param>
        /// <param name="geometryElements">Any extra geometry elements (i.e. altitudeMode).</param>        
        /// <param name="placemarkElements">Any extra placemark elements (i.e. styleUrl).</param>
        /// <returns></returns>
        public static XElement ToKml(this Feature feature, string name, double z = 0, XElement[] geometryElements = null, XElement[] placemarkElements = null)
        {
            return new XElement(kml + "Placemark",
                       new XElement(kml + "name", name),
                       placemarkElements,
                       new XElement(kml + "ExtendedData",
                           from n in feature.AllFieldNames
                           select new XElement(kml + "Data", new XAttribute("name", n),
                                      new XElement(kml + "value", feature[n]))),
                                          ToKml(((dynamic)feature).Geometry, z, geometryElements));
        }

        /// <summary>
        /// Converts the features to KML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="getPlacemark"></param>
        /// <param name="documentElements"></param>
        /// <returns></returns>
        public static XElement ToKml<T>(this IEnumerable<T> features, Func<T, XElement> getPlacemark, XElement[] documentElements = null) where T : Feature
        {
            return new XElement(kml + "kml",
                       new XElement(kml + "Document",
                           documentElements,
                           from f in features
                           select getPlacemark == null ? f.ToKml(f.OID.ToString()) : getPlacemark(f)));
        }

        /// <summary>
        /// Converts the features to KML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="getName"></param>
        /// <param name="getDescription"></param>
        /// <param name="getZ"></param>
        /// <param name="getStyleUrl"></param>
        /// <param name="documentElements"></param>
        /// <returns></returns>
        public static XElement ToKml<T>(this IEnumerable<T> features, Func<T, string> getName = null, Func<T, string> getDescription = null, Func<T, double> getZ = null, Func<T, string> getStyleUrl = null, XElement[] documentElements = null) where T : Feature
        {
            var geometryElements = getZ == null ? null : new[] { new XElement(kml + "extrude", 1), new XElement(kml + "altitudeMode", "relativeToGround") };
            var elements = new Dictionary<string, Func<T, object>> { { "description", getDescription }, { "styleUrl", getStyleUrl }, };

            return features.ToKml(f => f.ToKml(getName == null ? f.OID.ToString() : getName(f), getZ == null ? 0 : getZ(f), geometryElements, f.GetElements(elements)), documentElements);
        }
    }
}
