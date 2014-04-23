using System;
using System.Runtime.Serialization;

namespace PreStorm
{
    /// <summary>
    /// Represents errors that occur when handling a request against the ArcGIS Rest API.
    /// </summary>
    [Serializable]
    public class RestException : Exception
    {
        /// <summary>
        /// The url of the service.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// The data sent to the server.
        /// </summary>
        public string Request { get; private set; }

        /// <summary>
        /// The JSON response from ArcGIS Server.
        /// </summary>
        public string Response { get; private set; }

        /// <summary>
        /// Initializes a new instance of the RestException class.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RestException(string url, string request, string response, string message, Exception innerException)
            : base(message, innerException)
        {
            Url = url;
            Request = request;
            Response = response;
        }

        // ReSharper disable RedundantOverridenMember
        /// <summary>
        /// When overridden in a derived class, sets the System.Runtime.Serialization.SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
