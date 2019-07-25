using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cflashsoft.Framework.Http
{
    /// <summary>
    /// Represents response details sent by an HTTP API server.
    /// </summary>
    public class HttpApiResultDetails
    {
        //these are standard messages sent by ASP.NET WebAPI.
        #region Standard built-in ASP.NET WebAPI message properties

        /// <summary>
        /// API message text.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        /// API message details.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MessageDetail { get; set; }

        #endregion

        //these properties are automatically sent by ASP.NET WebAPI to return ModelState validation errors are such as when
        //CreateErrorResponse(HttpStatusCode, ModelState) or BadRequest(ModelState) is used.
        #region Built-in ASP.NET WebAPI ModelState model

        /// <summary>
        /// Model state validation messages and errors.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, IList<string>> ModelState { get; set; }

        #endregion

        //these properties are automatically sent by ASP.NET WebAPI to return exceptions such as when InternalServerError(exception)
        //is used or an uncaught exception is thrown.
        #region Built-in ASP.NET WebAPI Exception model

        //ASP.NET WebApi's exception model also includes a "Message" property already defined above in ModelState.

        /// <summary>
        /// Exception message text.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionMessage { get; set; }

        /// <summary>
        /// Exception .NET type name.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ExceptionType { get; set; }

        /// <summary>
        /// Stack trace for the exception. NOTE: this information is not populated by ASP.NET when debug = false.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StackTrace { get; set; }

        #endregion

        //these properties are typical error models returned by popular 3rd party REST API servers such as Facebook.
        #region Typical errors model

        /// <summary>
        /// API message code.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        /// <summary>
        /// A collection of errors returned from the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Errors { get; set; }

        /// <summary>
        /// An error string or object returned from the server.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Error { get; set; }

        #endregion

        /// <summary>
        /// Returns an HttpApiResultDetails instance converted from a JSON.NET JToken with similar fields.
        /// </summary>
        public static HttpApiResultDetails FromJToken(JToken rootToken)
        {
            HttpApiResultDetails result = new HttpApiResultDetails();

            if (rootToken != null)
            {
                try
                {
                    if (rootToken["Message"] != null)
                        result.Message = (string)rootToken["Message"];
                    else if (rootToken["message"] != null)
                        result.Message = (string)rootToken["message"];
                    else if (rootToken["msg"] != null)
                        result.Message = (string)rootToken["msg"];

                    if (rootToken["Code"] != null)
                        result.Code = (string)rootToken["Code"];
                    else if (rootToken["code"] != null)
                        result.Code = (string)rootToken["code"];

                    if (rootToken["MessageDetail"] != null)
                        result.MessageDetail = (string)rootToken["MessageDetail"];

                    if (rootToken["ExceptionMessage"] != null)
                        result.ExceptionMessage = (string)rootToken["ExceptionMessage"];

                    if (rootToken["ExceptionType"] != null)
                        result.ExceptionType = (string)rootToken["ExceptionType"];

                    if (rootToken["StackTrace"] != null)
                        result.StackTrace = (string)rootToken["StackTrace"];

                    if (rootToken["ModelState"] != null)
                        result.ModelState = rootToken["ModelState"].ToObject<Dictionary<string, IList<string>>>();

                    JToken errorsToken = null;

                    if (rootToken["errors"] != null)
                        errorsToken = rootToken["errors"];
                    else if (rootToken["error"] != null)
                        errorsToken = rootToken["error"];
                    else if (rootToken["Errors"] != null)
                        errorsToken = rootToken["Errors"];
                    else if (rootToken["Error"] != null)
                        errorsToken = rootToken["Error"];

                    if (errorsToken != null)
                    {
                        if (errorsToken is JArray)
                        {
                            result.Errors = new List<object>();

                            JArray errors = (JArray)errorsToken;

                            foreach (JToken token in errors)
                            {
                                result.Errors.Add(token);
                            }
                        }
                        else
                        {
                            result.Error = errorsToken;
                        }
                    }
                }
                catch { }
            }

            return result;
        }
    }
}
