using System.Collections.Generic;

namespace PreStorm
{
    /// <summary>
    /// Abstracts the querying of features.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// The maximum number of features returned by the server.  This information may not be available for older versions of ArcGIS Server.
        /// </summary>
        int? MaxRecordCount { get; }

        /// <summary>
        /// The url of the service.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// The feature layers and tables exposed by this service.
        /// </summary>
        Layer[] Layers { get; }

        /// <summary>
        /// The array of coded value domains used by this service.
        /// </summary>
        Domain[] Domains { get; }

        /// <summary>
        /// Downloads records and yields them as a lazy sequence of features of the specified type.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<T> Download<T>(int layerId, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature;

        /// <summary>
        /// Downloads records and yields them as a lazy sequence of features of the specified type.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="geometry">The geometry used to spatially filter the records.</param>
        /// <param name="spatialRel">The spatial relationship used for filtering.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<T> Download<T>(int layerId, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature;

        /// <summary>
        /// Downloads records and yields them as a lazy sequence of features of the specified type.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<T> Download<T>(string layerName, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature;

        /// <summary>
        /// Downloads records and yields them as a lazy sequence of features of the specified type.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="geometry">The geometry used to spatially filter the records.</param>
        /// <param name="spatialRel">The spatial relationship used for filtering.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<T> Download<T>(string layerName, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature;

        /// <summary>
        /// Downloads and yields features whose attributes and geometry are dynamically accessed at runtime.  Possibly throws RestException.
        /// </summary>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<DynamicFeature> Download(int layerId, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1);

        /// <summary>
        /// Downloads and yields features whose attributes and geometry are dynamically accessed at runtime.  Possibly throws RestException.
        /// </summary>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="geometry">The geometry used to spatially filter the records.</param>
        /// <param name="spatialRel">The spatial relationship used for filtering.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<DynamicFeature> Download(int layerId, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1);

        /// <summary>
        /// Downloads and yields features whose attributes and geometry are dynamically accessed at runtime.  Possibly throws RestException.
        /// </summary>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<DynamicFeature> Download(string layerName, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1);

        /// <summary>
        /// Downloads and yields features whose attributes and geometry are dynamically accessed at runtime.  Possibly throws RestException.
        /// </summary>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="geometry">The geometry used to spatially filter the records.</param>
        /// <param name="spatialRel">The spatial relationship used for filtering.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        IEnumerable<DynamicFeature> Download(string layerName, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1);
    }
}
