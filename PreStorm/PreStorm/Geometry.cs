namespace PreStorm
{
    /// <summary>
    /// Represents the base class for all geometry types that are supported by ArcGIS Rest API.
    /// </summary>
    public abstract class Geometry
    {
        /// <summary>
        /// Returns the JSON representation of the geometry.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Serialize();
        }
    }

    /// <summary>
    /// Represents the point geometry.
    /// </summary>
    public class Point : Geometry
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
        /// Deserializes the JSON string into a Point object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static implicit operator Point(string json)
        {
            return json.Deserialize<Point>();
        }
    }

    /// <summary>
    /// Represents the multipoint geometry.
    /// </summary>
    public class Multipoint : Geometry
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
            return json.Deserialize<Multipoint>();
        }
    }

    /// <summary>
    /// Represents the polyline geometry.
    /// </summary>
    public class Polyline : Geometry
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
            return json.Deserialize<Polyline>();
        }
    }

    /// <summary>
    /// Represents the polygon geometry.
    /// </summary>
    public class Polygon : Geometry
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
            return json.Deserialize<Polygon>();
        }
    }
}
