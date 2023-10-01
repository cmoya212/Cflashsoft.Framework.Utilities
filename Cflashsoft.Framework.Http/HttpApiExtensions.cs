using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cflashsoft.Framework.Http
{
    /// <summary>
    /// Extensions for HttpClient to simplify typical API Send and Read commands and retrieve API errors and go fuck yourself.
    /// </summary>
    public static class HttpApiExtensions
    {
        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <returns>Depending on TResult, a concrete class that the content can deserialize to.</returns>
        /// <remarks>Special handling is given to JToken, String, and Byte Array TResult. Non-generic methods for those 3 types also exist in these extensions. See ApiAsJTokenAsync, ApiAsStringAsync, and ApiAsByteArrayAsync.</remarks>
        public static Task<HttpApiResult<TResult>> ApiAsAsync<TResult>(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<TResult>(client, verb, requestUri, null, authenticationHeader, headers, HttpContentType.NotSet);
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="value">The data to post or put (not valid for get and delete requests)</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <param name="contentType">The HttpContentType to use when sending the data.</param>
        /// <returns>Depending on TResult, a concrete class that the content can deserialize to.</returns>
        /// <remarks>Special handling is given to JToken, String, and Byte Array TResult. Non-generic methods for those 3 types also exist in these extensions. See ApiAsJTokenAsync, ApiAsStringAsync, and ApiAsByteArrayAsync.</remarks>
        public static async Task<HttpApiResult<TResult>> ApiAsAsync<TResult>(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            using (HttpRequestMessage message = HttpApiUtility.CreateRequestMessage(verb, requestUri, value, contentType, authenticationHeader, headers))
            {
                try
                {
                    using (HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead))
                    {
                        return await ReadAsApiResponseAsync<TResult>(response.Content, response.StatusCode, response.IsSuccessStatusCode, response.Headers);
                    }
                }
                catch (HttpRequestException ex)
                {
                    return new HttpApiResult<TResult>()
                    {
                        StatusCode = HttpStatusCode.ServiceUnavailable,
                        IsSuccessStatusCode = false,
                        ClientException = ex
                    };
                }
            }
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line and receive a JToken dictionary of the result.
        /// </summary>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <returns>A JToken dictionary.</returns>
        public static Task<HttpApiResult<JToken>> ApiAsJTokenAsync(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<JToken>(client, verb, requestUri, authenticationHeader, headers);
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line and receive a JToken dictionary of the result.
        /// </summary>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="value">The data to post or put (not valid for get and delete requests)</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <param name="contentType">The HttpContentType to use when sending the data.</param>
        /// <returns>A JToken dictionary.</returns>
        public static Task<HttpApiResult<JToken>> ApiAsJTokenAsync(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            return ApiAsAsync<JToken>(client, verb, requestUri, value, authenticationHeader, headers, contentType);
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line and receive the result as a string.
        /// </summary>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <returns>A string containing the contents of the response.</returns>
        public static Task<HttpApiResult<string>> ApiAsStringAsync(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<string>(client, verb, requestUri, authenticationHeader, headers);
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line and receive the result as a string.
        /// </summary>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="value">The data to post or put (not valid for get and delete requests)</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <param name="contentType">The HttpContentType to use when sending the data.</param>
        /// <returns>A string containing the contents of the response.</returns>
        public static Task<HttpApiResult<string>> ApiAsStringAsync(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            return ApiAsAsync<string>(client, verb, requestUri, value, authenticationHeader, headers, contentType);
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line and receive the result as a byte array.
        /// </summary>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <returns>A byte array.</returns>
        public static Task<HttpApiResult<byte[]>> ApiAsByteArrayAsync(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<byte[]>(client, verb, requestUri, authenticationHeader, headers);
        }

        /// <summary>
        /// Utility function for HttpClient to perform an HTTP request in one line and receive the result as a byte array.
        /// </summary>
        /// <param name="client">The HttpClient.</param>
        /// <param name="verb">The HTTP verb to perform.</param>
        /// <param name="requestUri">The request endpoint.</param>
        /// <param name="value">The data to post or put (not valid for get and delete requests)</param>
        /// <param name="authenticationHeader">Authentication header to attach to the request.</param>
        /// <param name="headers">Misc headers to attach to the request.</param>
        /// <param name="contentType">The HttpContentType to use when sending the data.</param>
        /// <returns>A byte array.</returns>
        public static Task<HttpApiResult<byte[]>> ApiAsByteArrayAsync(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            return ApiAsAsync<byte[]>(client, verb, requestUri, value, authenticationHeader, headers, contentType);
        }

        /// <summary>
        /// Intelligently read the HttpContent as a particular class of object.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="content">The HttpContent of the response.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="isSuccessStatusCode">Indicates whether the HTTP status code should be considered a success status code.</param>
        /// <param name="responseHeaders">The headers returned in the response.</param>
        /// <returns>Will return either 1) a JToken object representing the contents, 2) a plain string of the contents, 3) a byte array, 4) or a concrete class that the content can deserialize to.</returns>
        /// <exception cref="FormatException"></exception>
        public static async Task<HttpApiResult<TResult>> ReadAsApiResponseAsync<TResult>(this HttpContent content, HttpStatusCode statusCode, bool isSuccessStatusCode, HttpResponseHeaders responseHeaders)
        {
            HttpApiResult<TResult> result = null;

            if (content != null)
            {
                Type resultType = typeof(TResult);

                if (typeof(JToken).IsAssignableFrom(resultType))
                {
                    //Deserializing to JToken

                    if (resultType != typeof(JToken))
                        throw new FormatException("Use JToken base type instead of JObject or JArray.");

                    JToken rootToken = await ReadAsJTokenAsync(content, isSuccessStatusCode, true, true);

                    result = new HttpApiResult<TResult>();

                    result.Details = HttpApiResultDetails.FromJToken(rootToken);

                    (result as HttpApiResult<JToken>).Value = rootToken;
                }
                else if (resultType == typeof(string))
                {
                    //Deserializing to String

                    result = new HttpApiResult<TResult>();

                    (result as HttpApiResult<string>).Value = await content.ReadAsStringAsync();
                }
                else if (resultType == typeof(byte[]))
                {
                    //Deserializing to Byte array

                    result = new HttpApiResult<TResult>();

                    (result as HttpApiResult<byte[]>).Value = await content.ReadAsByteArrayAsync();
                }
                else
                {
                    //Deserializing to T

                    await content.LoadIntoBufferAsync();

                    HttpApiResultDetails details = null;

                    try { details = await content.ReadAsAsync<HttpApiResultDetails>(); }
                    catch { }

                    (await content.ReadAsStreamAsync()).Seek(0, SeekOrigin.Begin);

                    TResult value = await content.ReadAsAsync<TResult>();

                    result = new HttpApiResult<TResult>(value, details);
                }
            }

            if (result == null)
            {
                result = new HttpApiResult<TResult>();
            }

            result.StatusCode = statusCode;
            result.IsSuccessStatusCode = isSuccessStatusCode;

            if (responseHeaders != null)
                result.ResponseHeaders = responseHeaders.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value)).ToList();

            return result;
        }

        /// <summary>
        /// Deserialize the HttpContent into a JToken hierarchy dictionary.
        /// </summary>
        /// <param name="content">The HttpContent of the response.</param>
        /// <returns>A JToken dictionary.</returns>
        public static Task<JToken> ReadAsJTokenAsync(this HttpContent content)
        {
            return ReadAsJTokenAsync(content, true, false);
        }

        /// <summary>
        /// Deserialize the HttpContent into a JToken hierarchy dictionary.
        /// </summary>
        /// <param name="content">The HttpContent of the response.</param>
        /// <param name="isSuccessStatusCode">Indicates whether the HTTP status code should be considered a success status code.</param>
        /// <param name="ignoreParsingErrorOnBadStatusCode"></param>
        /// <param name="ignoreAllParsingErrors"></param>
        /// <returns>A JToken dictionary.</returns>
        public static async Task<JToken> ReadAsJTokenAsync(this HttpContent content, bool isSuccessStatusCode, bool ignoreParsingErrorOnBadStatusCode, bool ignoreAllParsingErrors = false)
        {
            JToken result = null;

            if (content != null)
            {
                try
                {
                    if (content.Headers.ContentType != null && content.Headers.ContentType.MediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase))
                    {
                        XmlDocument xmlDocument = new XmlDocument();

                        using (Stream stream = await content.ReadAsStreamAsync())
                            xmlDocument.Load(stream);

                        string json = JsonConvert.SerializeXmlNode(xmlDocument);

                        result = JToken.Parse(json);
                    }
                    else
                    {
                        using (Stream stream = await content.ReadAsStreamAsync())
                        using (StreamReader reader = new StreamReader(stream))
                        using (JsonTextReader textReader = new JsonTextReader(reader))
                        {
                            result = await JToken.LoadAsync(textReader);
                        }
                    }
                }
                catch (JsonReaderException)
                {
                    if (isSuccessStatusCode && !ignoreAllParsingErrors)
                    {
                        throw; //this is unexpected and should not happen
                    }
                    else
                    {
                        if (ignoreParsingErrorOnBadStatusCode || ignoreAllParsingErrors)
                        {
                            //do nothing
                            //result = new JObject();
                            //result["RawServerResponse"] = contentValue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            return result;
        }

        //public static async Task<JToken> ReadAsJTokenAsync(this HttpContent content, bool isSuccessStatusCode, bool ignoreParsingErrorOnBadStatusCode)
        //{
        //    JToken result = null;

        //    if (content != null)
        //    {
        //        string contentValue = await content.ReadAsStringAsync();

        //        if (!string.IsNullOrEmpty(contentValue))
        //        {
        //            try
        //            {
        //                string json = null;

        //                if (content.Headers.ContentType != null && content.Headers.ContentType.MediaType.Equals("application/xml", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    XmlDocument xmlDocument = new XmlDocument();

        //                    try { xmlDocument.LoadXml(contentValue); json = JsonConvert.SerializeXmlNode(xmlDocument); }
        //                    catch { json = null; }
        //                }
        //                else
        //                {
        //                    json = contentValue;
        //                }

        //                if (!string.IsNullOrEmpty(json))
        //                    result = JToken.Parse(json);
        //            }
        //            catch (JsonReaderException)
        //            {
        //                if (isSuccessStatusCode)
        //                {
        //                    throw; //this is unexpected and should not happen
        //                }
        //                else
        //                {
        //                    if (ignoreParsingErrorOnBadStatusCode)
        //                    {
        //                        result = new JObject();

        //                        result["RawServerResponse"] = contentValue;
        //                    }
        //                    else
        //                    {
        //                        throw;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}
    }
}
