using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PreStorm
{
    /// <summary>
    /// Abstracts the querying of features.
    /// </summary>
    public class Service : IService
    {
        internal readonly ServiceArgs ServiceArgs;

        private readonly int? _maxRecordCount;

        /// <summary>
        /// The url of the service.
        /// </summary>
        public string Url => ServiceArgs.Url;

        /// <summary>
        /// The feature layers and tables exposed by this service.
        /// </summary>
        public Layer[] Layers { get; }

        /// <summary>
        /// The array of coded value domains used by this service.
        /// </summary>
        public Domain[] Domains { get; }

        private Service(string url, ICredentials credentials, Token token, string gdbVersion)
        {
            ServiceArgs = new ServiceArgs(url, credentials, token, gdbVersion);

            var serviceInfo = Esri.GetServiceInfo(ServiceArgs);

            _maxRecordCount = serviceInfo.maxRecordCount;

            Layers = serviceInfo.AllLayers;
            Domains = serviceInfo.AllDomains;
        }

        /// <summary>
        /// Initializes a new instance of the Service class.  Possibly throws RestException.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="credentials">The windows crendentials used for secured services.</param>
        /// <param name="gdbVersion">The geodatabase version.</param>
        public Service(string url, ICredentials credentials, string gdbVersion = null) : this(url, credentials, null, gdbVersion) { }

        /// <summary>
        /// Initializes a new instance of the Service class.  Possibly throws RestException.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="token">The authentication token.</param>
        /// <param name="gdbVersion">The geodatabase version.</param>
        public Service(string url, Token token, string gdbVersion = null) : this(url, null, token, gdbVersion) { }

        /// <summary>
        /// Initializes a new instance of the Service class.  Possibly throws RestException.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="gdbVersion">The geodatabase version.</param>
        public Service(string url, string gdbVersion = null) : this(url, null, null, gdbVersion) { }

        internal Layer GetLayer(int layerId)
        {
            var layer = Layers.FirstOrDefault(l => l.id == layerId);

            if (layer == null)
                throw new InvalidOperationException($"The service does not contain layer ID '{layerId}'.");

            return layer;
        }

        internal Layer GetLayer(string layerName)
        {
            var layers = Layers.Where(l => l.name == layerName).ToArray();

            switch (layers.Length)
            {
                case 1: return layers[0];
                case 0: throw new InvalidOperationException($"The service does not contain '{layerName}'.");
                default: throw new InvalidOperationException($"The service contains {layers.Length} layers called '{layerName}'.  Please try specifying the layer ID.");
            }
        }

        private T ToFeature<T>(Graphic graphic, Layer layer) where T : Feature
        {
            return graphic.ToFeature<T>(ServiceArgs, layer);
        }

        private IEnumerable<T> Download<T>(Layer layer, IEnumerable<int> objectIds, bool returnGeometry, string whereClause, string extraParameters, int batchSize, int degreeOfParallelism) where T : Feature
        {
            return objectIds.Partition(batchSize)
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(degreeOfParallelism < 1 ? 1 : degreeOfParallelism)
                .SelectMany(ids => Esri.GetFeatureSet(ServiceArgs, layer.id, returnGeometry, layer.hasZ, whereClause, extraParameters, ids).features
                    .Select(g => ToFeature<T>(g, layer)));
        }

        internal IEnumerable<T> Download<T>(int layerId, IEnumerable<int> objectIds, string whereClause, string extraParameters, int batchSize, int degreeOfParallelism) where T : Feature
        {
            var layer = GetLayer(layerId);
            var returnGeometry = typeof(T).HasGeometry();

            return Download<T>(layer, objectIds, returnGeometry, whereClause, extraParameters, batchSize, degreeOfParallelism);
        }

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
        public IEnumerable<T> Download<T>(int layerId, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            var layer = GetLayer(layerId);
            var returnGeometry = typeof(T).HasGeometry();

            var featureSet = Esri.GetFeatureSet(ServiceArgs, layerId, returnGeometry, layer.hasZ, whereClause, extraParameters, null);

            foreach (var g in featureSet.features)
                yield return ToFeature<T>(g, layer);

            var objectIds = featureSet.features.Select(g => Convert.ToInt32(g.attributes[layer.GetObjectIdFieldName()])).ToArray();

            if (!keepQuerying || objectIds.Length == 0 || _maxRecordCount > objectIds.Length)
                yield break;

            var remainingObjectIds = Esri.GetOIDSet(ServiceArgs, layerId, whereClause, extraParameters).objectIds.Except(objectIds);

            foreach (var f in Download<T>(layer, remainingObjectIds, returnGeometry, whereClause, extraParameters, objectIds.Length, degreeOfParallelism))
                yield return f;
        }

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
        public IEnumerable<T> Download<T>(int layerId, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            if (geometry == null)
                return Download<T>(layerId, whereClause, extraParameters, keepQuerying, degreeOfParallelism);

            string geometryType;

            if (geometry is Point)
                geometryType = "esriGeometryPoint";
            else if (geometry is Multipoint)
                geometryType = "esriGeometryMultipoint";
            else if (geometry is Polyline)
                geometryType = "esriGeometryPolyline";
            else if (geometry is Polygon)
                geometryType = "esriGeometryPolygon";
            else if (geometry is Envelope)
                geometryType = "esriGeometryEnvelope";
            else
                throw new ArgumentException("This geometry type is not supported.", nameof(geometry));

            var spatialFilter = $"geometry={Compatibility.UrlEncode(geometry.ToString())}&geometryType={geometryType}&spatialRel=esriSpatialRel{spatialRel}";

            return Download<T>(layerId, whereClause, string.IsNullOrEmpty(extraParameters) ? spatialFilter : (extraParameters + "&" + spatialFilter), keepQuerying, degreeOfParallelism);
        }

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
        public IEnumerable<T> Download<T>(string layerName, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            return Download<T>(GetLayer(layerName).id, whereClause, extraParameters, keepQuerying, degreeOfParallelism);
        }

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
        public IEnumerable<T> Download<T>(string layerName, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            return Download<T>(GetLayer(layerName).id, whereClause, geometry, spatialRel, extraParameters, keepQuerying, degreeOfParallelism);
        }

        /// <summary>
        /// Downloads and yields features whose attributes and geometry are dynamically accessed at runtime.  Possibly throws RestException.
        /// </summary>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        public IEnumerable<DynamicFeature> Download(int layerId, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1)
        {
            return Download<DynamicFeature>(layerId, whereClause, extraParameters, keepQuerying, degreeOfParallelism);
        }

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
        public IEnumerable<DynamicFeature> Download(int layerId, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1)
        {
            return Download<DynamicFeature>(layerId, whereClause, geometry, spatialRel, extraParameters, keepQuerying, degreeOfParallelism);
        }

        /// <summary>
        /// Downloads and yields features whose attributes and geometry are dynamically accessed at runtime.  Possibly throws RestException.
        /// </summary>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause.  If set to null, returns all features.</param>
        /// <param name="extraParameters">The query string that describes any additional query parameters (i.e. outSR=4326).  Each parameter must be url encoded.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        public IEnumerable<DynamicFeature> Download(string layerName, string whereClause = null, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1)
        {
            return Download<DynamicFeature>(layerName, whereClause, extraParameters, keepQuerying, degreeOfParallelism);
        }

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
        public IEnumerable<DynamicFeature> Download(string layerName, string whereClause, GeometryBase geometry, SpatialRel spatialRel, string extraParameters = null, bool keepQuerying = false, int degreeOfParallelism = 1)
        {
            return Download<DynamicFeature>(layerName, whereClause, geometry, spatialRel, extraParameters, keepQuerying, degreeOfParallelism);
        }
    }
}
