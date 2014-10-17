namespace PreStorm
{
    /// <summary>
    /// The spatial relationship used for server-side filtering of records.
    /// </summary>
    public enum SpatialRel
    {
        /// <summary>
        /// Corresponds to esriSpatialRelIntersects.
        /// </summary>
        Intersects = 1,

        /// <summary>
        /// Corresponds to esriSpatialRelEnvelopeIntersects.
        /// </summary>
        EnvelopeIntersects = 2,

        /// <summary>
        /// Corresponds to esriSpatialRelIndexIntersects.
        /// </summary>
        IndexIntersects = 3,

        /// <summary>
        /// Corresponds to esriSpatialRelTouches.
        /// </summary>
        Touches = 4,

        /// <summary>
        /// Corresponds to esriSpatialRelOverlaps.
        /// </summary>
        Overlaps = 5,

        /// <summary>
        /// Corresponds to esriSpatialRelCrosses.
        /// </summary>
        Crosses = 6,

        /// <summary>
        /// Corresponds to esriSpatialRelWithin.
        /// </summary>
        Within = 7,

        /// <summary>
        /// Corresponds to esriSpatialRelContains.
        /// </summary>
        Contains = 8
    }
}
