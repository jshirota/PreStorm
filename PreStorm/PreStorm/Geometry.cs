﻿using System.Collections.Generic;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Represents the base class for all geometry types that are supported by ArcGIS Rest API.
    /// </summary>
    public abstract class Geometry
    {
        /// <summary>
        /// The spatial reference of this geometry.
        /// </summary>
        public SpatialReference spatialReference { get; set; }

        internal static Envelope GetEnvelopeFromPoints(double[][] points)
        {
            return new Envelope
            {
                xmin = points.Min(p => p[0]),
                ymin = points.Min(p => p[1]),
                xmax = points.Max(p => p[0]),
                ymax = points.Max(p => p[1])
            };
        }

        internal static Envelope GetEnvelopeFromPaths(double[][][] paths)
        {
            return GetEnvelopeFromPoints(paths.SelectMany(r => r).ToArray());
        }

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var json = this.Serialize();

            var dictionary = json.Deserialize<Dictionary<string, object>>();
            var key = "spatialReference";

            if (dictionary.ContainsKey(key) && dictionary[key] == null)
                dictionary.Remove(key);

            return dictionary.Serialize();
        }
    }

    /// <summary>
    /// Represents the spatial reference.
    /// </summary>
    public class SpatialReference
    {
        /// <summary>
        /// The well-known id.
        /// </summary>
        public int wkid { get; set; }
    }

    /// <summary>
    /// Represents the point geometry.
    /// </summary>
    public sealed class Point : Geometry
    {
        /// <summary>
        /// The X coordinate.
        /// </summary>
        public double x { get; set; }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public double y { get; set; }

        /// <summary>
        /// Initializes a new instance of the Point class.
        /// </summary>
        public Point() { }

        /// <summary>
        /// Initializes a new instance of the Point class.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Deserializes the JSON string into a Point object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static implicit operator Point(string json)
        {
            return json == null ? null : json.Deserialize<Point>();
        }

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static implicit operator string(Point point)
        {
            return point == null ? null : point.ToString();
        }
    }

    /// <summary>
    /// Represents the multipoint geometry.
    /// </summary>
    public sealed class Multipoint : Geometry
    {
        /// <summary>
        /// The array of points.
        /// </summary>
        public double[][] points { get; set; }

        /// <summary>
        /// Deserializes the JSON string into a Multipoint object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static implicit operator Multipoint(string json)
        {
            return json == null ? null : json.Deserialize<Multipoint>();
        }

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <param name="multipoint"></param>
        /// <returns></returns>
        public static implicit operator string(Multipoint multipoint)
        {
            return multipoint == null ? null : multipoint.ToString();
        }

        /// <summary>
        /// The extent of this geometry.
        /// </summary>
        public Envelope Extent { get { return GetEnvelopeFromPoints(points); } }
    }

    /// <summary>
    /// Represents the polyline geometry.
    /// </summary>
    public sealed class Polyline : Geometry
    {
        /// <summary>
        /// The array of paths.
        /// </summary>
        public double[][][] paths { get; set; }

        /// <summary>
        /// Deserializes the JSON string into a Polyline object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static implicit operator Polyline(string json)
        {
            return json == null ? null : json.Deserialize<Polyline>();
        }

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static implicit operator string(Polyline polyline)
        {
            return polyline == null ? null : polyline.ToString();
        }

        /// <summary>
        /// The extent of this geometry.
        /// </summary>
        public Envelope Extent { get { return GetEnvelopeFromPaths(paths); } }
    }

    /// <summary>
    /// Represents the polygon geometry.
    /// </summary>
    public sealed class Polygon : Geometry
    {
        /// <summary>
        /// The array of rings.
        /// </summary>
        public double[][][] rings { get; set; }

        /// <summary>
        /// Deserializes the JSON string into a Polygon object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static implicit operator Polygon(string json)
        {
            return json == null ? null : json.Deserialize<Polygon>();
        }

        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        public static implicit operator string(Polygon polygon)
        {
            return polygon == null ? null : polygon.ToString();
        }

        /// <summary>
        /// The extent of this geometry.
        /// </summary>
        public Envelope Extent { get { return GetEnvelopeFromPaths(rings); } }
    }

    /// <summary>
    /// Represents the envelope geometry.
    /// </summary>
    public class Envelope
    {
        /// <summary>
        /// The minimum X coordinate.
        /// </summary>
        public double xmin { get; set; }

        /// <summary>
        /// The minimum Y coordinate.
        /// </summary>
        public double ymin { get; set; }

        /// <summary>
        /// The maximum X coordinate.
        /// </summary>
        public double xmax { get; set; }

        /// <summary>
        /// The maximum Y coordinate.
        /// </summary>
        public double ymax { get; set; }
    }
}
