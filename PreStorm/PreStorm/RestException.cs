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
        public string DataText { get; private set; }

        /// <summary>
        /// The HTTP method (GET or POST).
        /// </summary>
        public string HttpMethod { get; private set; }

        /// <summary>
        /// The JSON response from ArcGIS Server.
        /// </summary>
        public string ResponseJson { get; private set; }

        /// <summary>
        /// Initializes a new instance of the RestException class.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dataText"></param>
        /// <param name="httpMethod"></param>
        /// <param name="responseJson"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public RestException(string url, string dataText, string httpMethod, string responseJson, string message, Exception innerException)
            : base(message, innerException)
        {
            Url = url;
            DataText = dataText;
            HttpMethod = httpMethod;
            ResponseJson = responseJson;
        }

        // ReSharper disable RedundantOverridenMember
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
