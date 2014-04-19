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
        /// <param name="addResults"></param>
        /// <returns></returns>
        public static T[] InsertInto<T>(this T[] features, Service service, int layerId, out EditResult[] addResults) where T : Feature
        {
            if (features.Length == 0)
            {
                addResults = null;
                return new T[] { };
            }

            var layer = service.GetLayer(layerId);

            var adds = features.Select(f => f.ToGraphic(layer, false)).ToArray();

            var editResultInfo = Esri.ApplyEdits(service.Identity, layer.id, "adds", adds.Serialize());

            addResults = editResultInfo.addResults;

            return addResults == null || addResults.Any(r => !r.success)
                ? new T[] { }
                : service.Download<T>(layerId, editResultInfo.addResults.Select(r => r.objectId), 50, 1).ToArray();
        }

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
            EditResult[] addResults;
            return features.InsertInto(service, layerId, out addResults);
        }

        /// <summary>
        /// Inserts the feature into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <param name="addResult"></param>
        /// <returns></returns>
        public static T InsertInto<T>(this T feature, Service service, int layerId, out EditResult addResult) where T : Feature
        {
            EditResult[] addResults;
            var result = new[] { feature }.InsertInto(service, layerId, out addResults);
            addResult = addResults == null ? null : addResults.SingleOrDefault();
            return result.SingleOrDefault();
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
            EditResult addResult;
            return feature.InsertInto(service, layerId, out addResult);
        }

        /// <summary>
        /// Inserts the features into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <param name="addResults"></param>
        /// <returns></returns>
        public static T[] InsertInto<T>(this T[] features, Service service, string layerName, out EditResult[] addResults) where T : Feature
        {
            return features.InsertInto(service, service.GetLayer(layerName).id, out addResults);
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
        /// Inserts the features into a layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <param name="addResult"></param>
        /// <returns></returns>
        public static T InsertInto<T>(this T feature, Service service, string layerName, out EditResult addResult) where T : Feature
        {
            return feature.InsertInto(service, service.GetLayer(layerName).id, out addResult);
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
            return feature.InsertInto(service, service.GetLayer(layerName).id);
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
        /// <param name="updateResults"></param>
        /// <returns></returns>
        public static bool Update<T>(this T[] features, out EditResult[] updateResults) where T : Feature
        {
            if (features.Length == 0)
            {
                updateResults = null;
                return true;
            }

            if (features.Any(f => !f.IsDataBound))
                throw new Exception("All features must be bound to a data source before updating.");

            var identity = GetUnique(features, f => f.Identity, "url and geodatabase version");
            var layer = GetUnique(features, f => f.Layer, "layer");

            var updates = features.Select(f => f.ToGraphic(layer, true)).Where(o => o != null).ToArray();

            if (updates.Length == 0)
            {
                updateResults = null;
                return true;
            }

            var editResultInfo = Esri.ApplyEdits(identity, layer.id, "updates", updates.Serialize());

            updateResults = editResultInfo.updateResults;

            if (updateResults == null || updateResults.Any(r => !r.success))
                return false;

            foreach (var f in features)
                f.IsDirty = false;

            return true;
        }

        /// <summary>
        /// Updates the features in the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static bool Update<T>(this T[] features) where T : Feature
        {
            EditResult[] updateResults;
            return features.Update(out updateResults);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="updateResult"></param>
        /// <returns></returns>
        public static bool Update<T>(this T feature, out EditResult updateResult) where T : Feature
        {
            if (!feature.IsDataBound)
                throw new Exception("The feature cannot be updated because it is not bound to a data source.");

            EditResult[] updateResults;
            var result = new[] { feature }.Update(out updateResults);
            updateResult = updateResults == null ? null : updateResults.SingleOrDefault();
            return result;
        }

        /// <summary>
        /// Updates the feature in the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static bool Update<T>(this T feature) where T : Feature
        {
            EditResult updateResult;
            return feature.Update(out updateResult);
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
        /// <param name="deleteResults"></param>
        /// <returns></returns>
        public static bool Delete<T>(this T[] features, out EditResult[] deleteResults) where T : Feature
        {
            if (features.Length == 0)
            {
                deleteResults = null;
                return true;
            }

            if (features.Any(f => !f.IsDataBound))
                throw new Exception("All features must be bound to a data source before deleting.");

            var identity = GetUnique(features, f => f.Identity, "url and geodatabase version");
            var layer = GetUnique(features, f => f.Layer, "layer");

            var deletes = string.Join(",", features.Select(f => f.OID));

            var editResultInfo = Esri.ApplyEdits(identity, layer.id, "deletes", deletes);

            deleteResults = editResultInfo.deleteResults;

            if (deleteResults == null || deleteResults.Any(r => !r.success))
                return false;

            foreach (var f in features)
            {
                f.Identity = null;
                f.Layer = null;
                f.OID = -1;
                f.IsDirty = false;
            }

            return true;
        }

        /// <summary>
        /// Deletes the features from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static bool Delete<T>(this T[] features) where T : Feature
        {
            EditResult[] deleteResults;
            return features.Delete(out deleteResults);
        }

        /// <summary>
        /// Deletes the feature from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="deleteResult"></param>
        /// <returns></returns>
        public static bool Delete<T>(this T feature, out EditResult deleteResult) where T : Feature
        {
            if (!feature.IsDataBound)
                throw new Exception("The feature cannot be deleted because it is not bound to a data source.");

            EditResult[] deleteResults;
            var result = new[] { feature }.Delete(out deleteResults);
            deleteResult = deleteResults == null ? null : deleteResults.SingleOrDefault();
            return result;
        }

        /// <summary>
        /// Deletes the feature from the underlying layer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static bool Delete<T>(this T feature) where T : Feature
        {
            EditResult deleteResult;
            return feature.Delete(out deleteResult);
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
