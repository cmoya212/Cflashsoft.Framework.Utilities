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
    /// Extensions for HttpClient to simplify typical API Send and Read commands and retrieve API errors.
    /// </summary>
    public static class HttpApiExtensions
    {
        public static Task<HttpApiResult<TResult>> ApiAsAsync<TResult>(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<TResult>(client, verb, requestUri, null, authenticationHeader, headers, HttpContentType.NotSet);
        }

        public static async Task<HttpApiResult<TResult>> ApiAsAsync<TResult>(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            using (HttpRequestMessage message = HttpApiUtility.CreateRequestMessage(verb, requestUri, value, contentType, authenticationHeader, headers))
            {
                try
                {
                    using (HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead))
                    {
                        return await ReadAsApiResponseAsync<TResult>(response.Content, response.StatusCode, response.IsSuccessStatusCode);
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

        public static Task<HttpApiResult<JToken>> ApiAsJTokenAsync(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<JToken>(client, verb, requestUri, authenticationHeader, headers);
        }

        public static Task<HttpApiResult<JToken>> ApiAsJTokenAsync(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            return ApiAsAsync<JToken>(client, verb, requestUri, value, authenticationHeader, headers, contentType);
        }

        public static Task<HttpApiResult<string>> ApiAsStringAsync(this HttpClient client, HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return ApiAsAsync<string>(client, verb, requestUri, authenticationHeader, headers);
        }

        public static Task<HttpApiResult<string>> ApiAsStringAsync(this HttpClient client, HttpVerb verb, string requestUri, object value, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null, HttpContentType contentType = HttpContentType.Json)
        {
            return ApiAsAsync<string>(client, verb, requestUri, value, authenticationHeader, headers, contentType);
        }

        public static async Task<HttpApiResult<TResult>> ReadAsApiResponseAsync<TResult>(this HttpContent content, HttpStatusCode statusCode, bool isSuccessStatusCode)
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

            return result;
        }

        public static Task<JToken> ReadAsJTokenAsync(this HttpContent content)
        {
            return ReadAsJTokenAsync(content, true, false);
        }

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
