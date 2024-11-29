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
using Newtonsoft.Json.Serialization;

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

        public static HttpRequestMessage CreateRequestMessage(HttpVerb verb, string requestUri, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
        {
            return CreateRequestMessage(verb, requestUri, (object)null, HttpContentType.NotSet, authenticationHeader);
        }

        public static HttpRequestMessage CreateRequestMessage(HttpVerb verb, string requestUri, object value, HttpContentType contentType = HttpContentType.Json, AuthenticationHeaderValue authenticationHeader = null, IEnumerable<(string Key, string Value)> headers = null)
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

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    result.Headers.Add(header.Key, header.Value);
                }
            }

            return result;
        }

        public static HttpContent GetFormattedContent(object value, HttpContentType contentType)
        {
            if (value is HttpContent)
            {
                return value as HttpContent;
            }
            else
            {
                switch (contentType)
                {
                    case HttpContentType.Form:
                        return GetFormUrlEncodedContent(value);
                    case HttpContentType.CamelJson:
                    case HttpContentType.Json:
                    case HttpContentType.Xml:
                    case HttpContentType.Bson:
                        return GetFormattedObjectContent(value, contentType);
                    case HttpContentType.Multipart:
                        return GetMultipartFormDataContent(value);
                    case HttpContentType.NotSet:
                        return GetFormattedObjectContent(value, HttpContentType.CamelJson);
                        //throw new FormatException("When content type is not set, content must be an HttpContent derived object.");
                    default:
                        throw new ArgumentOutOfRangeException("ContentType is not recognized.");
                }
            }
        }

        private static FormUrlEncodedContent GetFormUrlEncodedContent(object value)
        {
            if (value != null)
            {
                Type type = value.GetType();

                if (typeof(FormUrlEncodedContent).IsAssignableFrom(type))
                {
                    return (FormUrlEncodedContent)value;
                }
                else if (typeof(IEnumerable<KeyValuePair<string, string>>).IsAssignableFrom(type))
                {
                    return new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)value);
                }
                else if (typeof(IEnumerable<(string, string)>).IsAssignableFrom(type))
                {
                    return new FormUrlEncodedContent(((IEnumerable<(string, string)>)value).Select(i => new KeyValuePair<string, string>(i.Item1, i.Item2)).ToList());
                }
                else
                {
                    return new FormUrlEncodedContent(type.GetProperties().Select(p => new KeyValuePair<string, string>(p.Name, p.GetValue(value).ToString())).ToList());
                }
            }
            else
            {
                return new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>());
            }
        }

        private static MultipartFormDataContent GetMultipartFormDataContent(object value)
        {
            if (value != null)
            {
                if (value is MultipartFormDataContent)
                {
                    return (MultipartFormDataContent)value;
                }
                else if (value is IEnumerable<(HttpContent, string, string)>)
                {
                    var result = new MultipartFormDataContent();

                    foreach (var item in (IEnumerable<(HttpContent, string, string)>)value)
                    {
                        result.Add(item.Item1, item.Item2, item.Item3);
                    }

                    return result;
                }
                else
                {
                    throw new ArgumentException("Unsupported object type.");
                }
            }
            else
            {
                return new MultipartFormDataContent();
            }
        }

        private static HttpContent GetFormattedObjectContent(object value, HttpContentType contentType)
        {
            if (value != null)
            {
                Type type = value.GetType();
                
                if (typeof(JToken).IsAssignableFrom(type))
                {
                    switch (contentType)
                    {
                        case HttpContentType.Json:
                            return new StringContent(((JToken)value).ToString(), Encoding.UTF8, "application/json");
                        case HttpContentType.Xml:
                            return new StringContent(JsonConvert.DeserializeXmlNode(((JToken)value).ToString()).OuterXml, Encoding.UTF8, "application/xml");
                        case HttpContentType.String:
                            return new StringContent(((JToken)value).ToString(), Encoding.UTF8);
                        //TODO: Add JToken to Bson conversion?
                        default:
                            throw new ArgumentOutOfRangeException("ContentType is not recognized for the provided input type.");
                    }
                }
                else if (value is string)
                {
                    switch (contentType)
                    {
                        case HttpContentType.Json:
                            return new StringContent(value as string, Encoding.UTF8, "application/json");
                        case HttpContentType.Xml:
                            return new StringContent(value as string, Encoding.UTF8, "application/xml");
                        default:
                            return new StringContent(value as string);
                    }
                }
                else
                {
                    switch (contentType)
                    {
                        case HttpContentType.CamelJson:
                            var formatter = new JsonMediaTypeFormatter();
                            formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                            return new ObjectContent(type, value, formatter);
                        case HttpContentType.Json:
                            return new ObjectContent(type, value, new JsonMediaTypeFormatter());
                        case HttpContentType.Xml:
                            return new ObjectContent(type, value, new XmlMediaTypeFormatter());
                        case HttpContentType.Bson:
                            return new ObjectContent(type, value, new BsonMediaTypeFormatter());
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
                case "multipart/form-data":
                    return HttpContentType.Multipart;
                default:
                    throw new ArgumentOutOfRangeException("ContentType is not recognized.");
            }
        }

        public static Cookie ParseCookieHeaderValue(string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
                return null;

            string[] properties = headerValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            properties = properties.Select(p => p.Trim()).ToArray();

            if (properties == null || properties.Length == 0)
                return null;

            int firstIndex = properties[0].IndexOf('=');

            if (firstIndex < 0)
                return null;

            Cookie cookie = new Cookie(properties[0].Substring(0, firstIndex).Trim(), properties[0].Substring(firstIndex + 1, properties[0].Length - firstIndex - 1).Trim());

            for (int index = 1; index < properties.Length; index++)
            {
                string property = properties[index].Trim();

                if (property.Equals("secure", StringComparison.OrdinalIgnoreCase))
                {
                    cookie.Secure = true;
                }
                else if (property.Equals("httponly", StringComparison.OrdinalIgnoreCase))
                {
                    cookie.HttpOnly = true;
                }
                else
                {
                    firstIndex = property.IndexOf('=');

                    if (firstIndex >= 0)
                    {
                        string key = property.Substring(0, firstIndex).Trim();
                        string value = property.Substring(firstIndex + 1, property.Length - firstIndex - 1).Trim();

                        if (key.Equals("path", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Path = value;
                        }
                        else if (key.Equals("expires", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Expires = DateTime.Parse(value);
                        }
                        else if (key.Equals("domain", StringComparison.OrdinalIgnoreCase))
                        {
                            cookie.Domain = value;
                        }
                    }
                }
            }

            return cookie;
        }
    }
}
