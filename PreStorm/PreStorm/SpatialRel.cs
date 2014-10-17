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
        Intersects,

        /// <summary>
        /// Corresponds to esriSpatialRelContains.
        /// </summary>
        Contains,

        /// <summary>
        /// Corresponds to esriSpatialRelCrosses.
        /// </summary>
        Crosses,

        /// <summary>
        /// Corresponds to esriSpatialRelEnvelopeIntersects.
        /// </summary>
        EnvelopeIntersects,

        /// <summary>
        /// Corresponds to esriSpatialRelIndexIntersects.
        /// </summary>
        IndexIntersects,

        /// <summary>
        /// Corresponds to esriSpatialRelOverlaps.
        /// </summary>
        Overlaps,

        /// <summary>
        /// Corresponds to esriSpatialRelTouches.
        /// </summary>
        Touches,

        /// <summary>
        /// Corresponds to esriSpatialRelWithin.
        /// </summary>
        Within
    }
}
