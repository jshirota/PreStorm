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
        /// Inserts the features into a layer and returns the newly created features.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static InsertResult<T> InsertInto<T>(this T[] features, Service service, int layerId) where T : Feature
        {
            try
            {
                if (features.Length == 0)
                    return new InsertResult<T>(true);

                var layer = service.GetLayer(layerId);

                var adds = features.Select(f => f.ToGraphic(layer, false)).ToArray();

                var editResultInfo = Esri.ApplyEdits(service.ServiceArgs, layer.id, "adds", adds.Serialize());

                var addedFeatures = service.Download<T>(layerId, editResultInfo.addResults.Select(r => r.objectId), null, null, 50, 1).ToArray();

                return new InsertResult<T>(true, null, addedFeatures);
            }
            catch (RestException restException)
            {
                return new InsertResult<T>(false, restException);
            }
        }

        /// <summary>
        /// Inserts the feature into a layer and returns the newly created feature.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static InsertResult<T> InsertInto<T>(this T feature, Service service, int layerId) where T : Feature
        {
            return new[] { feature }.InsertInto(service, layerId);
        }

        /// <summary>
        /// Inserts the features into a layer and returns the newly created features.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static InsertResult<T> InsertInto<T>(this T[] features, Service service, string layerName) where T : Feature
        {
            return features.InsertInto(service, service.GetLayer(layerName).id);
        }

        /// <summary>
        /// Inserts the feature into a layer and returns the newly created feature.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static InsertResult<T> InsertInto<T>(this T feature, Service service, string layerName) where T : Feature
        {
            return feature.InsertInto(service, service.GetLayer(layerName).id);
        }

        /// <summary>
        /// Inserts the features into a layer and returns the newly created features.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static Task<InsertResult<T>> InsertIntoAsync<T>(this T[] features, Service service, int layerId) where T : Feature
        {
            return Task<InsertResult<T>>.Factory.StartNew(() => features.InsertInto(service, layerId));
        }

        /// <summary>
        /// Inserts the feature into a layer and returns the newly created feature.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static Task<InsertResult<T>> InsertIntoAsync<T>(this T feature, Service service, int layerId) where T : Feature
        {
            return Task<InsertResult<T>>.Factory.StartNew(() => feature.InsertInto(service, layerId));
        }

        /// <summary>
        /// Inserts the features into a layer and returns the newly created features.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Task<InsertResult<T>> InsertIntoAsync<T>(this T[] features, Service service, string layerName) where T : Feature
        {
            return Task<InsertResult<T>>.Factory.StartNew(() => features.InsertInto(service, layerName));
        }

        /// <summary>
        /// Inserts the feature into a layer and returns the newly created feature.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="service"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static Task<InsertResult<T>> InsertIntoAsync<T>(this T feature, Service service, string layerName) where T : Feature
        {
            return Task<InsertResult<T>>.Factory.StartNew(() => feature.InsertInto(service, layerName));
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the features in the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static UpdateResult Update<T>(this T[] features) where T : Feature
        {
            try
            {
                if (features.Length == 0)
                    return new UpdateResult(true);

                if (features.Any(f => !f.IsDataBound))
                    throw new Exception("All features must be bound to a data source before they can be updated.");

                var args = GetUnique(features, f => f.ServiceArgs, "url and geodatabase version");
                var layer = GetUnique(features, f => f.Layer, "layer");

                var updates = features.Select(f => f.ToGraphic(layer, true)).Where(o => o != null).ToArray();

                if (updates.Length == 0)
                    return new UpdateResult(true);

                Esri.ApplyEdits(args, layer.id, "updates", updates.Serialize());

                foreach (var f in features)
                    f.IsDirty = false;

                return new UpdateResult(true);
            }
            catch (RestException restException)
            {
                return new UpdateResult(false, restException);
            }
        }

        /// <summary>
        /// Updates the feature in the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static UpdateResult Update<T>(this T feature) where T : Feature
        {
            return new[] { feature }.Update();
        }

        /// <summary>
        /// Updates the features in the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static Task<UpdateResult> UpdateAsync<T>(this T[] features) where T : Feature
        {
            return Task<UpdateResult>.Factory.StartNew(features.Update);
        }

        /// <summary>
        /// Updates the feature in the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static Task<UpdateResult> UpdateAsync<T>(this T feature) where T : Feature
        {
            return Task<UpdateResult>.Factory.StartNew(feature.Update);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the features from the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static DeleteResult Delete<T>(this T[] features) where T : Feature
        {
            try
            {
                if (features.Length == 0)
                    return new DeleteResult(true);

                if (features.Any(f => !f.IsDataBound))
                    throw new Exception("All features must be bound to a data source before they can be deleted.");

                var args = GetUnique(features, f => f.ServiceArgs, "url and geodatabase version");
                var layer = GetUnique(features, f => f.Layer, "layer");

                var deletes = string.Join(",", features.Select(f => f.OID));

                Esri.ApplyEdits(args, layer.id, "deletes", deletes);

                foreach (var f in features)
                {
                    f.ServiceArgs = null;
                    f.Layer = null;
                    f.OID = -1;
                    f.IsDirty = false;
                }

                return new DeleteResult(true);
            }
            catch (RestException restException)
            {
                return new DeleteResult(false, restException);
            }
        }

        /// <summary>
        /// Deletes the feature from the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static DeleteResult Delete<T>(this T feature) where T : Feature
        {
            return new[] { feature }.Delete();
        }

        /// <summary>
        /// Deletes the features from the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public static Task<DeleteResult> DeleteAsync<T>(this T[] features) where T : Feature
        {
            return Task<DeleteResult>.Factory.StartNew(features.Delete);
        }

        /// <summary>
        /// Deletes the feature from the underlying layer.  Possibly throws RestException.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static Task<DeleteResult> DeleteAsync<T>(this T feature) where T : Feature
        {
            return Task<DeleteResult>.Factory.StartNew(feature.Delete);
        }

        #endregion
    }
}
