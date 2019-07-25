using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cflashsoft.Framework.Http
{
    public static class HttpApiUtility
    {
        private static Lazy<HttpClient> _defaultHttpClient = new Lazy<HttpClient>(() => new HttpClient(new HttpClientHandler() { UseCookies = false }));
        private static Lazy<HttpClient> _jsonHttpClient = new Lazy<HttpClient>(() => CreateHttpClientWithAcceptHeader("application/json", useCookies: false));
        private static Lazy<HttpClient> _xmlHttpClient = new Lazy<HttpClient>(() => CreateHttpClientWithAcceptHeader("application/xml", useCookies: false));
        private static Lazy<HttpClient> _bsonHttpClient = new Lazy<HttpClient>(() => CreateHttpClientWithAcceptHeader("application/bson", useCookies: false));

        public static HttpClient DefaultHttpClient => _defaultHttpClient.Value;
        public static HttpClient JsonHttpClient => _jsonHttpClient.Value;
        public static HttpClient XmlHttpClient => _xmlHttpClient.Value;
        public static HttpClient BsonHttpClient => _bsonHttpClient.Value;

        public static HttpClient CreateHttpClientWithAcceptHeader(string acceptHeader, bool useCookies = true)
        {
            HttpClient result = null;

            if (useCookies)
                result = new HttpClient();
            else
                result = new HttpClient(new HttpClientHandler() { UseCookies = false });

            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
       
            return result;
        }

        public static HttpRequestMessage CreateRequestMessage(HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null)
        {
            return CreateRequestMessage(verb, requestUri, (object)null, HttpContentType.NotSet, authenticationHeader);
        }

        public static HttpRequestMessage CreateRequestMessage(HttpVerb verb, string requestUri, object value, HttpContentType contentType = HttpContentType.Json, AuthenticationHeaderValue authenticationHeader = null)
        {
            if ((value != null || contentType != HttpContentType.NotSet) && (verb == HttpVerb.Get || verb == HttpVerb.Delete))
                throw new ArgumentException("Get and Delete requests cannot contain message bodies and ContentType is not applicable.");

            HttpRequestMessage result = null;

            switch (verb)
            {
                case HttpVerb.Get:
                    result = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    break;
                case HttpVerb.Post:
                    result = new HttpRequestMessage(HttpMethod.Post, requestUri);
                    result.Content = GetFormattedContent(value, contentType);
                    break;
                case HttpVerb.Put:
                    result = new HttpRequestMessage(HttpMethod.Put, requestUri);
                    result.Content = GetFormattedContent(value, contentType);
                    break;
                case HttpVerb.Patch:
                    result = new HttpRequestMessage(ExtendedHttpMethod.Patch, requestUri);
                    result.Content = GetFormattedContent(value, contentType);
                    break;
                case HttpVerb.Delete:
                    result = new HttpRequestMessage(HttpMethod.Delete, requestUri);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unsupported HTTP verb specified.");
            }

            if (authenticationHeader != null)
                result.Headers.Authorization = authenticationHeader;

            return result;
        }

        public static HttpContent GetFormattedContent(object value, HttpContentType contentType)
        {
            switch (contentType)
            {
                case HttpContentType.Form:
                    return new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)value);
                case HttpContentType.Json:
                case HttpContentType.Xml:
                case HttpContentType.Bson:
                    return GetFormattedObjectContent(value, contentType);
                default:
                    throw new ArgumentOutOfRangeException("ContentType is not recognized.");
            }
        }

        private static HttpContent GetFormattedObjectContent(object value, HttpContentType contentType)
        {
            if (value != null)
            {
                Type valueType = value.GetType();

                if (typeof(JToken).IsAssignableFrom(valueType))
                {
                    switch (contentType)
                    {
                        case HttpContentType.Json:
                            return new StringContent(((JToken)value).ToString());
                        case HttpContentType.Xml:
                            return new StringContent(JsonConvert.DeserializeXmlNode(((JToken)value).ToString()).OuterXml);
                        //TODO: Add JToken to Bson conversion?
                        default:
                            throw new ArgumentOutOfRangeException("ContentType is not recognized for the provided input type.");
                    }
                }
                else if (value is string)
                {
                    return new StringContent(value as string);
                }
                else
                {
                    switch (contentType)
                    {
                        case HttpContentType.Json:
                            return new ObjectContent(valueType, value, new JsonMediaTypeFormatter());
                        case HttpContentType.Xml:
                            return new ObjectContent(valueType, value, new XmlMediaTypeFormatter());
                        case HttpContentType.Bson:
                            return new ObjectContent(valueType, value, new BsonMediaTypeFormatter());
                        default:
                            throw new ArgumentOutOfRangeException("ContentType is not recognized for the provided input type.");
                    }
                }
            }
            else
            {
                return new StringContent(string.Empty);
            }
        }

        public static HttpContentType GetContentType(string contentType)
        {
            switch (contentType.ToLower())
            {
                case "application/x-www-form-urlencoded":
                    return HttpContentType.Form;
                case "application/json":
                    return HttpContentType.Json;
                case "application/xml":
                    return HttpContentType.Xml;
                case "application/bson":
                    return HttpContentType.Bson;
                default:
                    throw new ArgumentOutOfRangeException("ContentType is not recognized.");
            }
        }
    }
}
