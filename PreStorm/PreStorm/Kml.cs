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

        private static string ToCoordinates(this Point geometry, double? z)
        {
            return geometry.x + "," + geometry.y + "," + (z ?? (geometry.z ?? 0));
        }

        private static string ToCoordinates(this double[] coordinates, double? z)
        {
            return coordinates[0] + "," + coordinates[1] + "," + (z ?? coordinates.ElementAtOrDefault(2));
        }

        private static string ToCoordinates(this double[][] coordinates, double? z)
        {
            return string.Join(" ", coordinates.Select(c => c.ToCoordinates(z)));
        }

        private static XElement ToKmlPoint(this Point geometry, double? z, XElement[] extraElements)
        {
            return new XElement(kml + "Point", extraElements,
                new XElement(kml + "coordinates", geometry.ToCoordinates(z)));
        }

        private static XElement ToKmlMultipoint(this Multipoint geometry, double? z, XElement[] extraElements)
        {
            return new XElement(kml + "MultiGeometry",
                geometry.points.Select(p => new Point { x = p[0], y = p[1], z = p.Length > 2 ? p[2] : (double?)null }.ToKmlPoint(z, extraElements)));
        }

        private static XElement ToKmlPolyline(this Polyline geometry, double? z, XElement[] extraElements)
        {
            return new XElement(kml + "MultiGeometry",
                geometry.paths.Select(p =>
                    new XElement(kml + "LineString", extraElements,
                        new XElement(kml + "coordinates", p.ToCoordinates(z)))));
        }

        private static XElement ToKmlPolygon(this Polygon geometry, double? z, XElement[] extraElements)
        {
            var polygons = new List<XElement>();

            foreach (var ring in geometry.rings)
            {
                var isInnerRing = ring.IsInnerRing();

                if (!isInnerRing)
                    polygons.Add(new XElement(kml + "Polygon", extraElements));

                polygons.Last().Add(new XElement(kml + (isInnerRing ? "innerBoundaryIs" : "outerBoundaryIs"),
                    new XElement(kml + "LinearRing",
                        new XElement(kml + "coordinates", ring.ToCoordinates(z)))));
            }

            return new XElement(kml + "MultiGeometry", polygons);
        }

        /// <summary>
        /// Converts the style object to KML.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public static XElement ToKml(this KmlStyle style)
        {
            return new XElement(kml + "Style", new XAttribute("id", style.GetHashCode()),
                       new XElement(kml + "IconStyle",
                           new XElement(kml + "color", style.IconColour),
                           new XElement(kml + "scale", style.IconScale),
                           new XElement(kml + "Icon", style.IconUrl)),
                       new XElement(kml + "LineStyle",
                           new XElement(kml + "color", style.LineColour),
                           new XElement(kml + "width", style.LineWidth)),
                       new XElement(kml + "PolyStyle",
                           new XElement(kml + "color", style.PolygonColour)));
        }

        /// <summary>
        /// Converts the geometry to KML.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="z">The altitude in meters.</param>
        /// <param name="geometryElements">Any extra geometry elements (i.e. altitudeMode).</param>
        /// <returns></returns>
        public static XElement ToKml(this Geometry geometry, double? z = null, params XElement[] geometryElements)
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
        public static XElement ToKml(this Feature feature, string name = null, double? z = null, XElement[] geometryElements = null, params XElement[] placemarkElements)
        {
            return new XElement(kml + "Placemark", new XAttribute("id", feature.OID),
                       new XElement(kml + "name", name), placemarkElements,
                       new XElement(kml + "ExtendedData",
                           from n in feature.AllFieldNames
                           select new XElement(kml + "Data", new XAttribute("name", n),
                                      new XElement(kml + "value", feature[n]))),
                                          ToKml(((dynamic)feature).Geometry, z, geometryElements));
        }

        /// <summary>
        /// Converts the feature to KML.
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="name"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static XElement ToKml(this Feature feature, string name = null, KmlStyle style = null)
        {
            return feature.ToKml(name, 0, null, style == null ? null : style.ToKml());
        }

        /// <summary>
        /// Converts the features to KML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="name">The function that returns the name for the placemark.</param>
        /// <param name="placemarkElements">Any extra placemark elements (i.e. styleUrl).</param>
        /// <param name="documentElements">Any extra document elements (i.e. Style).</param>
        /// <returns></returns>
        public static XElement ToKml<T>(this IEnumerable<T> features, Func<T, string> name = null, Func<T, XElement[]> placemarkElements = null, params XElement[] documentElements) where T : Feature
        {
            return new XElement(kml + "kml",
                       new XElement(kml + "Document", documentElements,
                           features.Select(f => f.ToKml(name == null ? null : name(f), null, null, placemarkElements == null ? null : placemarkElements(f)))));
        }

        /// <summary>
        /// Converts the features to KML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="name">The function that returns the name for the placemark.</param>
        /// <param name="style">The function that returns the style for the placemark.</param>
        /// <returns></returns>
        public static XElement ToKml<T>(this IEnumerable<T> features, Func<T, string> name, Func<T, KmlStyle> style) where T : Feature
        {
            if (style == null)
                return features.ToKml(name, null, null);

            var dictionary = features.Distinct().ToDictionary(f => f, style);

            var styles = dictionary.Values.Distinct().Select(ToKml).ToArray();

            return dictionary.Keys.ToKml(name, f => new[] { new XElement(kml + "styleUrl", "#" + dictionary[f].GetHashCode()) }, styles);
        }
    }
}
