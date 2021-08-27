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
    /// Represents a result returned by an HTTP API server. NOTE: this is a client class used to deserialize and read various types of responses. It should not be used by an ASP.NET WebAPI server to return results. Instead use RESTful models with Ok(MyModel), BadRequest(ModelState), Content(HttpStatusCode, new { [Errors], [MyModel] }), etc.
    /// </summary>
    public class HttpApiResult<T>
    {
        /// <summary>
        /// Error information and exception details for the response.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public HttpApiResultDetails Details { get; set; }

        /// <summary>
        /// Contents of the response.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public T Value { get; set; }

        /// <summary>
        /// HTTP status code of the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Exception thrown by HttpClient. 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Exception ClientException { get; set; }

        /// <summary>
        /// Returns true if the HTTP status code represents a successful status.
        /// </summary>
        public bool IsSuccessStatusCode { get; set; }

        /// <summary>
        /// Returns true if HttpClient threw an exception during the request such as when a server was not found.
        /// </summary>
        public bool HasClientException => this.ClientException != null;

        /// <summary>
        /// Returns true if there are modelstate errors in the response that are typically returned by ASP.NET WebApi servers.
        /// </summary>
        public bool HasModelStateErrors => this.Details != null && this.Details.ModelState != null && this.Details.ModelState.Any() && this.Details.ModelState.Any(k => k.Value != null && k.Value.Any());

        /// <summary>
        /// Returns true if there are typical errors in the response returned by many API servers.
        /// </summary>
        public bool HasGenericErrors => this.Details != null && ((this.Details.Errors != null && this.Details.Errors.Any()) || this.Details.Error != null);

        /// <summary>
        /// Returns true if there are any errors in this response including non-200 http status, exceptions, modelstate errors, or typical json errors.
        /// </summary>
        public bool HasErrors => !this.IsSuccessStatusCode || this.HasClientException || this.HasModelStateErrors || this.HasGenericErrors;

        /// <summary>
        /// Initializes a new instance of the HttpApiResult class.
        /// </summary>
        public HttpApiResult()
        {

        }

        /// <summary>
        /// Initializes a new instance of the HttpApiResult class.
        /// </summary>
        public HttpApiResult(T value, HttpApiResultDetails details)
        {
            this.Value = value;
            this.Details = details;
        }

        /// <summary>
        /// Returns a flattened list of all errors including exceptions, model errors, and detected json error collections.
        /// </summary>
        public IEnumerable<string> GetErrors(bool detailed = false)
        {
            if (this.HasErrors)
            {
                IEnumerable<string> result = Enumerable.Empty<string>();

                if (this.ClientException != null)
                {
                    if (detailed)
                    {
                        result.Concat(new string[] { GetDetailedClientExceptionError() });
                    }
                    else
                    {
                        result.Concat(new string[] { GetClientExceptionError() });
                    }
                }
                else if (this.Details != null)
                {
                    if (!this.IsSuccessStatusCode)
                    {
                        if (detailed)
                        {
                            result = result.Concat(new string[] { GetDetailedStatusError() });
                        }
                        else
                        {
                            if (!this.HasModelStateErrors && !this.HasGenericErrors)
                                result = result.Concat(new string[] { GetStatusError() });
                        }
                    }

                    if (this.HasModelStateErrors)
                    {
                        result = result.Concat(GetModelStateErrors());
                    }

                    if (this.HasGenericErrors)
                    {
                        result = result.Concat(GetGenericErrors());
                    }
                }
                else
                {
                    result = result.Concat(new string[] { this.StatusCode.ToString() });
                }

                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the exception error message thrown by the HttpClient.
        /// </summary>
        public string GetClientExceptionError()
        {
            if (this.HasClientException)
            {
                return this.ClientException.Message;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the exception error message thrown by the HttpClient with details.
        /// </summary>
        public string GetDetailedClientExceptionError()
        {
            if (this.HasClientException)
            {
                return this.ClientException.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the HTTP status code description.
        /// </summary>
        public string GetStatusError()
        {
            if (this.Details != null)
            {
                if (!string.IsNullOrEmpty(this.Details.Message))
                {
                    return $"{this.StatusCode.ToString()} - {this.Details.Message}";
                }
                else
                {
                    return this.StatusCode.ToString();
                }
            }
            else
            {
                return this.StatusCode.ToString();
            }
        }

        /// <summary>
        /// Returns the HTTP status code description together with exception information if any.
        /// </summary>
        /// <returns></returns>
        public string GetDetailedStatusError()
        {
            if (this.Details != null)
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrEmpty(this.Details.Message))
                {
                    sb.Append(this.StatusCode.ToString());
                    sb.Append(" - ");
                    sb.AppendLine(this.Details.Message);
                }
                else
                {
                    sb.AppendLine(this.StatusCode.ToString());
                }

                if (!string.IsNullOrEmpty(this.Details.MessageDetail))
                    sb.AppendLine(this.Details.MessageDetail);

                if (!string.IsNullOrEmpty(this.Details.ExceptionType))
                    sb.AppendLine(this.Details.ExceptionType);

                if (!string.IsNullOrEmpty(this.Details.ExceptionMessage))
                    sb.AppendLine(this.Details.ExceptionMessage);

                if (!string.IsNullOrEmpty(this.Details.StackTrace))
                    sb.AppendLine(this.Details.StackTrace);

                return sb.ToString();
            }
            else
            {
                return this.StatusCode.ToString();
            }
        }

        /// <summary>
        /// Returns a flattened list of modelstate errors (typically returned by ASP.NET WebApi servers).
        /// </summary>
        public IEnumerable<string> GetModelStateErrors()
        {
            if (this.HasModelStateErrors)
            {
                return this.Details.ModelState.SelectMany(b => b.Value);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a list of generic errors (typically returned by many API servers).
        /// </summary>
        public IEnumerable<string> GetGenericErrors()
        {
            if (this.HasGenericErrors)
            {
                if (this.Details.Errors != null && this.Details.Errors.Any())
                {
                    return this.Details.Errors.Select(e => e.ToString());
                }
                else
                {
                    return new string[] { this.Details.Error.ToString() };
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Throws an exception if the api result contains errors.
        /// </summary>
        public void AssertSuccess()
        {
            if (this.HasErrors)
                throw new HttpApiException((int)this.StatusCode, string.Join("\r\n", GetErrors()));
        }

        /// <summary>
        /// Throws an exception if the api result contains errors.
        /// </summary>
        public void AssertSuccess(string message)
        {
            if (this.HasErrors)
                throw new HttpApiException(message, (int)this.StatusCode, string.Join("\r\n", GetErrors()));
        }

        /// <summary>
        /// Throws an exception if the api result is not an HTTP success.
        /// </summary>
        public void AssertIsSuccessStatusCode()
        {
            if (!this.IsSuccessStatusCode)
                throw new HttpApiException((int)this.StatusCode, string.Join("\r\n", GetErrors()));
        }

        /// <summary>
        /// Throws an exception if the api result is not an HTTP success.
        /// </summary>
        public void AssertIsSuccessStatusCode(string message)
        {
            if (!this.IsSuccessStatusCode)
                throw new HttpApiException(message, (int)this.StatusCode, string.Join("\r\n", GetErrors()));
        }

        /// <summary>
        /// Returns the api result value if no errors are present, otherwise returns a null or default value.
        /// </summary>
        public T GetValueOnSuccess(bool checkAllErrors = false)
        {
            if ((!checkAllErrors && this.IsSuccessStatusCode) || (checkAllErrors && !this.HasErrors))
                return this.Value;
            else
                return default(T);
        }
    }
}
