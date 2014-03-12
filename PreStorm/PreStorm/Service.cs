using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PreStorm
{
    /// <summary>
    /// Abstracts the querying of features.
    /// </summary>
    public class Service
    {
        internal readonly string Url;
        internal readonly ICredentials Credentials;
        internal readonly Token Token;
        private readonly Esri.Layer[] _layers;

        /// <summary>
        /// The array of coded value domains used by this service.
        /// </summary>
        public Domain[] Domains { get; private set; }

        private Service(string url, ICredentials credentials, string userName, string password)
        {
            Url = url;
            Credentials = credentials;

            if (userName != null)
                Token = new Token(url, userName, password);

            var serviceInfo = Esri.GetServiceInfo(url, credentials, Token);

            _layers = (serviceInfo.layers ?? new Esri.Layer[] { }).Concat(serviceInfo.tables ?? new Esri.Layer[] { }).ToArray();

            Domains = _layers
                .Where(l => l.fields != null)
                .SelectMany(l => l.fields)
                .Where(f => f.domain != null && f.domain.type == "codedValue")
                .GroupBy(d => d.name)
                .Select(g => g.First())
                .Select(d => new Domain { name = d.name, codedValues = d.domain.codedValues.Select(c => new CodedValue { code = c.code, name = c.name }).ToArray() })
                .ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="credentials">The windows crendentials used for secured services.</param>
        public Service(string url, ICredentials credentials) : this(url, credentials, null, null) { }

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        /// <param name="userName">The user name for token-based authentication.</param>
        /// <param name="password">The password for token-based authentication.</param>
        public Service(string url, string userName, string password) : this(url, null, userName, password) { }

        /// <summary>
        /// Initializes a new instance of the Service class.
        /// </summary>
        /// <param name="url">The url of the service.  The url should end with either MapServer or FeatureServer.</param>
        public Service(string url) : this(url, null, null, null) { }

        internal Esri.Layer GetLayer(int layerId)
        {
            var layer = _layers.FirstOrDefault(l => l.id == layerId);

            if (layer == null)
                throw new Exception(string.Format("The service does not contain layer ID '{0}'.", layerId));

            return layer;
        }

        internal Esri.Layer GetLayer(string layerName)
        {
            var layers = _layers.Where(l => l.name == layerName).ToArray();

            switch (layers.Length)
            {
                case 1: return layers[0];
                case 0: throw new Exception(string.Format("The service does not contain '{0}'.", layerName));
                default: throw new Exception(string.Format("The service contains {0} layers called '{1}'.  Please try specifying the layer ID.", layers.Length, layerName));
            }
        }

        private T ToFeature<T>(Esri.Graphic graphic, Esri.Layer layer) where T : Feature
        {
            var f = graphic.ToFeature<T>(layer);
            f.Url = Url;
            f.Credentials = Credentials;
            f.Token = Token;
            return f;
        }

        private IEnumerable<T> Download<T>(Esri.Layer layer, IEnumerable<int> objectIds, bool returnGeometry, int batchSize, int degreeOfParallelism) where T : Feature
        {
            return objectIds.Partition(batchSize)
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(degreeOfParallelism < 1 ? 1 : degreeOfParallelism)
                .SelectMany(ids => Esri.GetFeatureSet(Url, layer.id, Credentials, Token, returnGeometry, null, ids).features
                    .Select(g => ToFeature<T>(g, layer)));
        }

        internal IEnumerable<T> Download<T>(int layerId, IEnumerable<int> objectIds, int batchSize, int degreeOfParallelism) where T : Feature
        {
            var layer = GetLayer(layerId);
            var returnGeometry = typeof(T).HasGeometry();

            return Download<T>(layer, objectIds, returnGeometry, 100, degreeOfParallelism);
        }

        /// <summary>
        /// Downloads records and them as a lazy sequence of features of the specified type.
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

            var featureSet = Esri.GetFeatureSet(Url, layerId, Credentials, Token, returnGeometry, whereClause, null);

            foreach (var g in featureSet.features)
                yield return ToFeature<T>(g, layer);

            var objectIds = featureSet.features.Select(g => Convert.ToInt32(g.attributes[layer.GetObjectIdFieldName()])).ToArray();

            if (!keepQuerying || objectIds.Length == 0)
                yield break;

            var remainingObjectIds = Esri.GetOIDSet(Url, layerId, Credentials, Token, whereClause).objectIds.Except(objectIds);

            foreach (var f in Download<T>(layer, remainingObjectIds, returnGeometry, objectIds.Length, degreeOfParallelism))
                yield return f;
        }

        /// <summary>
        /// Downloads records and them as a lazy sequence of features of the specified type.
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
    }
}
