using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PreStorm
{
    /// <summary>
    /// Abstracts the querying of features.
    /// </summary>
    public class Service
    {
        internal readonly ICredentials Credentials;
        internal readonly Token Token;
        internal readonly string GdbVersion;

        /// <summary>
        /// The url of the service.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// The feature layers and tables exposed by this service.
        /// </summary>
        public Layer[] Layers { get; private set; }

        /// <summary>
        /// The array of coded value domains used by this service.
        /// </summary>
        public Domain[] Domains { get; private set; }

        private Service(string url, ICredentials credentials, string userName, string password, string gdbVersion)
        {
            Url = url;

            Credentials = credentials;

            if (userName != null)
                Token = new Token(url, userName, password);

            GdbVersion = gdbVersion;

            var serviceInfo = Esri.GetServiceInfo(url, credentials, Token, gdbVersion);

            Layers = (serviceInfo.layers ?? new Layer[] { })
                .Where(l => l.type == "Feature Layer")
                .Concat(serviceInfo.tables ?? new Layer[] { })
                .ToArray();

            Domains = Layers
                .Where(l => l.fields != null)
                .SelectMany(l => l.fields)
                .Select(f => f.domain)
                .Where(d => d != null && d.type == "codedValue")
                .GroupBy(d => d.name)
                .Select(g => g.First())
                .ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="credentials">The windows crendentials used for secured services.</param>
        /// <param name="gdbVersion">The geodatabase version.</param>
        public Service(string url, ICredentials credentials, string gdbVersion = null) : this(url, credentials, null, null, gdbVersion) { }

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="userName">The user name for token-based authentication.</param>
        /// <param name="password">The password for token-based authentication.</param>
        /// <param name="gdbVersion">The geodatabase version.</param>
        public Service(string url, string userName, string password, string gdbVersion = null) : this(url, null, userName, password, gdbVersion) { }

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="gdbVersion">The geodatabase version.</param>
        public Service(string url, string gdbVersion = null) : this(url, null, null, null, gdbVersion) { }

        internal Layer GetLayer(int layerId)
        {
            var layer = Layers.FirstOrDefault(l => l.id == layerId);

            if (layer == null)
                throw new Exception(string.Format("The service does not contain layer ID '{0}'.", layerId));

            return layer;
        }

        internal Layer GetLayer(string layerName)
        {
            var layers = Layers.Where(l => l.name == layerName).ToArray();

            switch (layers.Length)
            {
                case 1: return layers[0];
                case 0: throw new Exception(string.Format("The service does not contain '{0}'.", layerName));
                default: throw new Exception(string.Format("The service contains {0} layers called '{1}'.  Please try specifying the layer ID.", layers.Length, layerName));
            }
        }

        private T ToFeature<T>(Graphic graphic, Layer layer) where T : Feature
        {
            var f = graphic.ToFeature<T>(layer);
            f.Url = Url;
            f.Credentials = Credentials;
            f.Token = Token;
            return f;
        }

        private IEnumerable<T> Download<T>(Layer layer, IEnumerable<int> objectIds, bool returnGeometry, int batchSize, int degreeOfParallelism) where T : Feature
        {
            return objectIds.Partition(batchSize)
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(degreeOfParallelism < 1 ? 1 : degreeOfParallelism)
                .SelectMany(ids => Esri.GetFeatureSet(Url, layer.id, Credentials, Token, GdbVersion, returnGeometry, null, ids).features
                    .Select(g => ToFeature<T>(g, layer)));
        }

        internal IEnumerable<T> Download<T>(int layerId, IEnumerable<int> objectIds, int batchSize, int degreeOfParallelism) where T : Feature
        {
            var layer = GetLayer(layerId);
            var returnGeometry = typeof(T).HasGeometry();

            return Download<T>(layer, objectIds, returnGeometry, batchSize, degreeOfParallelism);
        }

        /// <summary>
        /// Downloads records and yields them as a lazy sequence of features of the specified type.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause for server-side filtering.  If set to null, returns all features.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        public IEnumerable<T> Download<T>(int layerId, string whereClause = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            var layer = GetLayer(layerId);
            var returnGeometry = typeof(T).HasGeometry();

            var featureSet = Esri.GetFeatureSet(Url, layerId, Credentials, Token, GdbVersion, returnGeometry, whereClause, null);

            foreach (var g in featureSet.features)
                yield return ToFeature<T>(g, layer);

            var objectIds = featureSet.features.Select(g => Convert.ToInt32(g.attributes[layer.GetObjectIdFieldName()])).ToArray();

            if (!keepQuerying || objectIds.Length == 0)
                yield break;

            var remainingObjectIds = Esri.GetOIDSet(Url, layerId, Credentials, Token, GdbVersion, whereClause).objectIds.Except(objectIds);

            foreach (var f in Download<T>(layer, remainingObjectIds, returnGeometry, objectIds.Length, degreeOfParallelism))
                yield return f;
        }

        /// <summary>
        /// Downloads records and yields them as a lazy sequence of features of the specified type.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause for server-side filtering.  If set to null, returns all features.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        public IEnumerable<T> Download<T>(string layerName, string whereClause = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            return Download<T>(GetLayer(layerName).id, whereClause, keepQuerying, degreeOfParallelism);
        }

        /// <summary>
        /// Downloads records and returns them as features of the specified type.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerId">The layer ID of the feature layer or table.</param>
        /// <param name="whereClause">The where clause for server-side filtering.  If set to null, returns all features.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        public Task<T[]> DownloadAsync<T>(int layerId, string whereClause = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            return Task<T[]>.Factory.StartNew(() => Download<T>(layerId, whereClause, keepQuerying, degreeOfParallelism).ToArray());
        }

        /// <summary>
        /// Downloads records and returns them as features of the specified type.
        /// </summary>
        /// <typeparam name="T">The type the record should be mapped to.</typeparam>
        /// <param name="layerName">The name of the feature layer or table.  If the service contains two or more layers with this name, use the overload that takes the layer ID rather than the name.</param>
        /// <param name="whereClause">The where clause for server-side filtering.  If set to null, returns all features.</param>
        /// <param name="keepQuerying">If set to true, repetitively queries the server until all features have been returned.</param>
        /// <param name="degreeOfParallelism">The maximum number of concurrent requests.</param>
        /// <returns></returns>
        public Task<T[]> DownloadAsync<T>(string layerName, string whereClause = null, bool keepQuerying = false, int degreeOfParallelism = 1) where T : Feature
        {
            return Task<T[]>.Factory.StartNew(() => Download<T>(layerName, whereClause, keepQuerying, degreeOfParallelism).ToArray());
        }
    }
}
