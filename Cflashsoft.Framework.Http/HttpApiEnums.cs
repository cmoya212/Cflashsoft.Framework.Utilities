using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Cflashsoft.Framework.Http
{
    /// <summary>
    /// HTTP verb enumeration.
    /// </summary>
    public enum HttpVerb
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Get
        /// </summary>
        Get = 1,
        /// <summary>
        /// Post
        /// </summary>
        Post = 2,
        /// <summary>
        /// Put
        /// </summary>
        Put = 3,
        /// <summary>
        /// Patch
        /// </summary>
        Patch = 4,
        /// <summary>
        /// Delete
        /// </summary>
        Delete = 5
    }

    /// <summary>
    /// Enumeration for media type formatters.
    /// </summary>
    public enum HttpContentType
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Form
        /// </summary>
        Form = 1,
        /// <summary>
        /// Json
        /// </summary>
        Json = 2,
        /// <summary>
        /// Xml
        /// </summary>
        Xml = 3,
        /// <summary>
        /// Bson
        /// </summary>
        Bson = 4
    }

    /// <summary>
    /// Additional HTTP methods that are not included by default with the standard Web API HttpMethod helper class.
    /// </summary>
    public static class ExtendedHttpMethod
    {
        /// <summary>
        /// Represents an HTTP PATCH protocol method that is used to modify an entity identified by a URI.
        /// </summary>
        public static HttpMethod Patch { get; } = new HttpMethod("PATCH");
    }
}
