using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PreStorm
{
    /// <summary>
    /// Provides editing capabilities to features.
    /// </summary>
    public static class Editor
    {
        private static TResult GetUnique<TSource, TResult>(IEnumerable<TSource> items, Func<TSource, TResult> selector, string name)
        {
            var values = items.Select(selector).Distinct().ToArray();

            if (values.Length > 1)
                throw new Exception(string.Format("All features must be bound to the same {0}.", name));

            return values.SingleOrDefault();
        }

        #region Insert

        /// <summary>
        /// Inserts the features into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static T[] InsertInto<T>(this T[] features, Service service, int layerId) where T : Feature
        {
            if (features.Length == 0)
                return new T[] { };

            var layer = service.GetLayer(layerId);

            var adds = features.Select(f => f.ToGraphic(layer, false)).ToArray();

            var editResultInfo = Esri.ApplyEdits(service.Url, layer.id, service.Credentials, service.Token, "adds", adds.Serialize());

            return editResultInfo.addResults.Any(r => !r.success)
                ? new T[] { }
                : service.Download<T>(layerId, editResultInfo.addResults.Select(r => r.objectId), 50, 1).ToArray();
        }

        /// <summary>
        /// Inserts the feature into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static T InsertInto<T>(this T feature, Service service, int layerId) where T : Feature
        {
            return new[] { feature }.InsertInto(service, layerId).FirstOrDefault();
        }

        /// <summary>
        /// Inserts the features into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static T[] InsertInto<T>(this T[] features, Service service, string layerName) where T : Feature
        {
            return features.InsertInto(service, service.GetLayer(layerName).id);
        }

        /// <summary>
        /// Inserts the feature into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static T InsertInto<T>(this T feature, Service service, string layerName) where T : Feature
        {
            return new[] { feature }.InsertInto(service, layerName).FirstOrDefault();
        }

        /// <summary>
        /// Inserts the features into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static Task<T[]> InsertIntoAsync<T>(this T[] features, Service service, int layerId) where T : Feature
        {
            return Task<T[]>.Factory.StartNew(() => features.InsertInto(service, layerId));
        }

        /// <summary>
        /// Inserts the feature into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static Task<T> InsertIntoAsync<T>(this T feature, Service service, int layerId) where T : Feature
        {
            return Task<T>.Factory.StartNew(() => feature.InsertInto(service, layerId));
        }

        /// <summary>
        /// Inserts the features into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Task<T[]> InsertIntoAsync<T>(this T[] features, Service service, string layerName) where T : Feature
        {
            return Task<T[]>.Factory.StartNew(() => features.InsertInto(service, layerName));
        }

        /// <summary>
        /// Inserts the feature into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Task<T> InsertIntoAsync<T>(this T feature, Service service, string layerName) where T : Feature
        {
            return Task<T>.Factory.StartNew(() => feature.InsertInto(service, layerName));
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the features in the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static bool Update<T>(this T[] features) where T : Feature
        {
            if (features.Length == 0)
                return true;

            if (features.Any(f => !f.IsDataBound))
                throw new Exception("All features must be bound to a data source before updating.");

            var url = GetUnique(features, f => f.Url, "url");
            var layer = GetUnique(features, f => f.Layer, "layer");
            var credentials = GetUnique(features, f => f.Credentials, "credentials");
            var token = GetUnique(features, f => f.Token, "token");

            var updates = features.Select(f => f.ToGraphic(layer, true)).Where(o => o != null).ToArray();

            if (updates.Length == 0)
                return true;

            var editResultInfo = Esri.ApplyEdits(url, layer.id, credentials, token, "updates", updates.Serialize());

            if (editResultInfo.updateResults.Any(r => !r.success))
                return false;

            foreach (var f in features)
                f.IsDirty = false;

            return true;
        }

        /// <summary>
        /// Updates the feature in the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static bool Update<T>(this T feature) where T : Feature
        {
            if (!feature.IsDataBound)
                throw new Exception("The feature cannot be updated because it is not bound to a data source.");

            return new[] { feature }.Update();
        }

        /// <summary>
        /// Updates the features in the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static Task<bool> UpdateAsync<T>(this T[] features) where T : Feature
        {
            return Task<bool>.Factory.StartNew(features.Update);
        }

        /// <summary>
        /// Updates the feature in the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static Task<bool> UpdateAsync<T>(this T feature) where T : Feature
        {
            return Task<bool>.Factory.StartNew(feature.Update);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the features from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static bool Delete<T>(this T[] features) where T : Feature
        {
            if (features.Length == 0)
                return true;

            if (features.Any(f => !f.IsDataBound))
                throw new Exception("All features must be bound to a data source before deleting.");

            var url = GetUnique(features, f => f.Url, "url");
            var layer = GetUnique(features, f => f.Layer, "layer");
            var credentials = GetUnique(features, f => f.Credentials, "credentials");
            var token = GetUnique(features, f => f.Token, "token");

            var deletes = string.Join(",", features.Select(f => f.OID));

            var editResultInfo = Esri.ApplyEdits(url, layer.id, credentials, token, "deletes", deletes);

            if (editResultInfo.deleteResults.Any(r => !r.success))
                return false;

            foreach (var f in features)
            {
                f.Url = null;
                f.Layer = null;
                f.OID = -1;
                f.Credentials = null;
                f.Token = null;
                f.IsDirty = false;
            }

            return true;
        }

        /// <summary>
        /// Deletes the feature from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static bool Delete<T>(this T feature) where T : Feature
        {
            if (!feature.IsDataBound)
                throw new Exception("The feature cannot be deleted because it is not bound to a data source.");

            return new[] { feature }.Delete();
        }

        /// <summary>
        /// Deletes the features from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static Task<bool> DeleteAsync<T>(this T[] features) where T : Feature
        {
            return Task<bool>.Factory.StartNew(features.Delete);
        }

        /// <summary>
        /// Deletes the feature from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static Task<bool> DeleteAsync<T>(this T feature) where T : Feature
        {
            return Task<bool>.Factory.StartNew(feature.Delete);
        }

        #endregion
    }
}
