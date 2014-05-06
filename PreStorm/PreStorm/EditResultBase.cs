using System;
using System.Linq;

namespace PreStorm
{
    /// <summary>
    /// Represents the base for editing results.
    /// </summary>
    public abstract class EditResultBase
    {
        /// <summary>
        /// Indicates if the request has been successful.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Any exception thrown while processing the request.
        /// </summary>
        public RestException RestException { get; private set; }

        /// <summary>
        /// Initializes a new instance of the EditResultBase class.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="restException"></param>
        protected EditResultBase(bool success, RestException restException)
        {
            Success = success;
            RestException = restException;
        }
    }

    /// <summary>
    /// Represents the result of an insert operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertResult<T> : EditResultBase
    {
        private readonly Lazy<T[]> _insertedFeatures;

        /// <summary>
        /// The inserted features.  In case of an error, returns an empty array.
        /// </summary>
        public T[] InsertedFeatures { get { return _insertedFeatures.Value; } }

        /// <summary>
        /// The inserted feature.  In case of an error, returns null.
        /// </summary>
        public T InsertedFeature
        {
            get { return GetSingleFeature(); }
        }

        private T GetSingleFeature()
        {
            if (InsertedFeatures.Length > 1)
                throw new Exception("There are more than one inserted features.");

            return InsertedFeatures.SingleOrDefault();
        }

        /// <summary>
        /// Initializes a new instance of the InsertResult class.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="restException"></param>
        /// <param name="getInsertedFeatures"></param>
        public InsertResult(bool success, RestException restException = null, Func<T[]> getInsertedFeatures = null)
            : base(success, restException)
        {
            _insertedFeatures = new Lazy<T[]>(() => getInsertedFeatures == null ? new T[] { } : getInsertedFeatures());
        }
    }

    /// <summary>
    /// Represents the result of an update operation.
    /// </summary>
    public class UpdateResult : EditResultBase
    {
        /// <summary>
        /// Initializes a new instance of the UpdateResult class.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="restException"></param>
        public UpdateResult(bool success, RestException restException = null)
            : base(success, restException)
        {
        }
    }

    /// <summary>
    /// Represents the result of a delete operation.
    /// </summary>
    public class DeleteResult : EditResultBase
    {
        /// <summary>
        /// Initializes a new instance of the DeleteResult class.
        /// </summary>
        /// <param name="success"></param>
        /// <param name="restException"></param>
        public DeleteResult(bool success, RestException restException = null)
            : base(success, restException)
        {
        }
    }
}
