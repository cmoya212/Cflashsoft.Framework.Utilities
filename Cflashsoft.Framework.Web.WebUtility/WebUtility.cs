using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Cflashsoft.Framework.Web
{
    /// <summary>
    /// A collection of utility methods for web requests.
    /// </summary>
    public static class WebUtility
    {
        /// <summary>
        /// Unix epoch timestamp start value (1970/1/1). 
        /// </summary>
        public static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static object _mimeTypesSyncLock = new object();

        static WebUtility()
        {

        }

        /// <summary>
        /// Convert a Unix timestamp to a .NET datetime.
        /// </summary>
        public static DateTime ConvertFromUnixTime(long unixTime)
        {
            return EPOCH.AddSeconds(unixTime);
        }

        /// <summary>
        /// Convert a .NET datetime to a Unix timestamp.
        /// </summary>
        public static long ConvertToUnixTime(DateTime date)
        {
            return Convert.ToInt64((date.ToUniversalTime() - EPOCH).TotalSeconds);
        }

        /// <summary>
        /// Convert a server datetime to browser client time by reading the timezoneOffset cookie.
        /// </summary>
        public static DateTime ConvertToClientTime(HttpContextBase context, DateTime date)
        {
            DateTime result = date;

            string timezoneOffsetValue = GetCookieValueOrDefault(context.Request.Cookies, "timezoneOffset");

            if (!string.IsNullOrEmpty(timezoneOffsetValue))
            {
                result = ConvertToClientTime(date, int.Parse(timezoneOffsetValue));
            }

            return result;
        }

        /// <summary>
        /// Convert a server datetime to browser client time using the specified offset.
        /// </summary>
        public static DateTime ConvertToClientTime(DateTime date, int timezoneOffset)
        {
            return date.AddMinutes(timezoneOffset);
        }

        /// <summary>
        /// Send an email using the default SmtpClient.
        /// </summary>
        public static void SendEmail(string address, string subject, string body)
        {
            SendEmail(new string[] { address }, subject, body);
        }

        /// <summary>
        /// Send an email using the default SmtpClient.
        /// </summary>
        public static void SendEmail(IEnumerable<string> addresses, string subject, string body)
        {
            SendEmail(addresses.Select(a => new MailAddress(a)), subject, body, null);
        }

        /// <summary>
        /// Send an email using the default SmtpClient. If debugEmailOverride contains addresses for debugging purposes, those addresses are used instead.
        /// </summary>
        public static void SendEmail(IEnumerable<MailAddress> addresses, string subject, string body, string debugEmailOverride)
        {
            List<MailAddress> effectiveAddresses = new List<MailAddress>();

            SetEffectiveEmailAddresses(addresses, effectiveAddresses, ref body, debugEmailOverride);

            using (SmtpClient smtp = new SmtpClient())
            {
                using (MailMessage msg = new MailMessage())
                {
                    foreach (MailAddress address in effectiveAddresses)
                    {
                        msg.To.Add(address);
                    }

                    msg.Subject = subject;
                    msg.IsBodyHtml = true;
                    msg.Body = body;

                    smtp.Send(msg);
                }
            }
        }

        /// <summary>
        /// Send an email using the default SmtpClient.
        /// </summary>
        public static async Task SendEmailAsync(string address, string subject, string body)
        {
            await SendEmailAsync(new string[] { address }, subject, body);
        }

        /// <summary>
        /// Send an email using the default SmtpClient.
        /// </summary>
        public static async Task SendEmailAsync(IEnumerable<string> addresses, string subject, string body)
        {
            await SendEmailAsync(addresses.Select(a => new MailAddress(a)), subject, body, null);
        }

        /// <summary>
        /// Send an email using the default SmtpClient. If debugEmailOverride contains addresses for debugging purposes, those addresses are used instead.
        /// </summary>
        public static async Task SendEmailAsync(IEnumerable<MailAddress> addresses, string subject, string body, string debugEmailOverride)
        {
            List<MailAddress> effectiveAddresses = new List<MailAddress>();

            SetEffectiveEmailAddresses(addresses, effectiveAddresses, ref body, null);

            using (SmtpClient smtp = new SmtpClient())
            {
                using (MailMessage msg = new MailMessage())
                {
                    foreach (MailAddress address in effectiveAddresses)
                    {
                        msg.To.Add(address);
                    }

                    msg.Subject = subject;
                    msg.IsBodyHtml = true;
                    msg.Body = body;

                    await smtp.SendMailAsync(msg);
                }
            }
        }

        private static void SetEffectiveEmailAddresses(IEnumerable<MailAddress> addresses, List<MailAddress> effectiveAddresses, ref string body, string debugEmailOverride)
        {
            if (!string.IsNullOrEmpty(debugEmailOverride))
            {
                string[] debugEmailOverrideParsed = debugEmailOverride.Split(new char[] { ';', ',' });
                StringBuilder sb = new StringBuilder();

                sb.Append("(Debug override: Original recipient(s): ");

                foreach (string debugAddress in debugEmailOverrideParsed)
                {
                    effectiveAddresses.Add(new MailAddress(debugAddress));
                }

                foreach (MailAddress address in addresses)
                {
                    sb.Append(address.Address);
                    sb.Append(",");
                }

                sb.Append(")<br /><br /><br />");

                body = sb.ToString() + body;
            }
            else
            {
                effectiveAddresses.AddRange(addresses);
            }
        }

        /// <summary>
        /// Serialize an object to XML.
        /// </summary>
        public static string SerializeToXmlString(object o)
        {
            return SerializeToXmlString(o, "Error serializing object");
        }

        /// <summary>
        /// Serialize an object to XML.
        /// </summary>
        public static string SerializeToXmlString(object o, string errorText)
        {
            string result = null;

            if (o != null)
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(o.GetType());

                    using (StringWriter writer = new StringWriter())
                    {
                        xmlSerializer.Serialize(writer, o);

                        result = writer.ToString();
                    }
                }
                catch (Exception ex)
                {
                    result = errorText + " " + o.GetType().ToString() + ": " + ex.ToString();
                }
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Deserialize an object from XML.
        /// </summary>
        public static T DeserializeFromXmlString<T>(string xmlText)
        {
            return (T)DeserializeFromXmlString(xmlText, typeof(T));
        }

        /// <summary>
        /// Deserialize an object from XML.
        /// </summary>
        public static Object DeserializeFromXmlString(string xmlText, Type targetType)
        {
            return DeserializeFromXmlString(new StringReader(xmlText), targetType);
        }

        /// <summary>
        /// Deserialize an object from XML.
        /// </summary>
        public static Object DeserializeFromXmlString(TextReader textReader, Type targetType)
        {
            Object result = null;

            System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(targetType);

            result = xmlSerializer.Deserialize(textReader);

            return result;
        }

        /// <summary>
        /// Convert an XML document to a Newtownsoft JSON.NET document.
        /// </summary>
        public static JToken ConvertXmlToJToken(string xmlText)
        {
            XmlDocument document = new XmlDocument();

            document.LoadXml(xmlText);

            return ConvertXmlToJToken(document);
        }

        /// <summary>
        /// Convert an XML document to a Newtownsoft JSON.NET document.
        /// </summary>
        public static JToken ConvertXmlToJToken(XmlDocument xmlDocument)
        {
            return JToken.Parse(JsonConvert.SerializeXmlNode(xmlDocument, Newtonsoft.Json.Formatting.None, true));
        }

        /// <summary>
        /// Convert an XML document to a Newtownsoft JSON.NET document.
        /// </summary>
        public static JToken ConvertXmlToJToken(Stream stream)
        {
            XmlDocument document = new XmlDocument();

            document.Load(stream);

            return ConvertXmlToJToken(document);
        }

        /// <summary>
        /// Get a cookie value from HttpCookieCollection or return a default value.
        /// </summary>
        public static string GetCookieValueOrDefault(this HttpCookieCollection cookies, string name)
        {
            string result = null;

            var cookie = cookies[name];

            if (cookie != null)
                result = cookie.Value;

            return result;
        }

        /// <summary>
        /// Write a collection of object into a stream as JSON.
        /// </summary>
        public static void WriteToStreamAsJsonArray(this IEnumerable enumerable, Stream outputStream)
        {
            using (StreamWriter sw = new StreamWriter(outputStream))
            {
                using (JsonTextWriter jw = new JsonTextWriter(sw))
                {
                    jw.WriteStartArray();

                    IEnumerator enumerator = enumerable.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        jw.WriteValue(enumerator.Current);
                    }

                    jw.WriteEndArray();
                }
            }
        }

        /// <summary>
        /// Create a StreamContent response that pulls from a stream and is disposed when the request ends. 
        /// </summary>
        public static HttpContent CreateJsonArrayPushStreamContent(IEnumerable enumerable)
        {
            return new PushStreamContent((outputStream, httpContent, transportContext) =>
            {
                WriteToStreamAsJsonArray(enumerable, outputStream);
            });
        }

        /// <summary>
        /// Get the official mimetype value of the specified file extension. 
        /// </summary>
        public static string GetMimeType(string extension)
        {
            string result = "application/octet-stream";

            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (extension.Substring(0, 1) == ".")
                    extension = extension.Substring(1, extension.Length - 1);

                Dictionary<string, string> mimeTypes = GetMimeTypesList();

                if (mimeTypes != null)
                {
                    if (!mimeTypes.TryGetValue(extension, out result))
                    {
                        result = mimeTypes["*"];
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get a list of file extensions and their associated mime type.
        /// </summary>
        public static Dictionary<string, string> GetMimeTypesList()
        {
            Dictionary<string, string> result = null;

            try
            {
                result = (Dictionary<string, string>)System.Runtime.Caching.MemoryCache.Default.Get("MimeTypes");

                if (result == null)
                {
                    lock (_mimeTypesSyncLock)
                    {
                        result = (Dictionary<string, string>)System.Runtime.Caching.MemoryCache.Default.Get("MimeTypes");

                        if (result == null)
                        {
                            result = new Dictionary<string, string>();

                            bool useInternal = false;

                            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "mimetypes.csv");

                            try
                            {
                                using (StreamReader reader = new StreamReader(filePath))
                                {
                                    ReadMimeTypes(reader);
                                }
                            }
                            catch
                            {
                                useInternal = true;
                            }

                            if (useInternal)
                            {
                                result.Clear();

                                using (StreamReader reader = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Cflashsoft.Framework.Web.mimetypes.csv")))
                                {
                                    ReadMimeTypes(reader);
                                }

                                System.Runtime.Caching.MemoryCache.Default.Add("MimeTypes", result, new System.Runtime.Caching.CacheItemPolicy());
                            }
                            else
                            {
                                System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy();

                                policy.ChangeMonitors.Add(new System.Runtime.Caching.HostFileChangeMonitor(new string[] { filePath }));

                                System.Runtime.Caching.MemoryCache.Default.Add("MimeTypes", result, policy);
                            }
                        }
                    }
                }
            }
            catch
            {
                //do nothing
            }

            void ReadMimeTypes(StreamReader reader)
            {
                while (reader.Peek() >= 0)
                {
                    string line = reader.ReadLine();

                    if (line != string.Empty)
                    {
                        string[] lineParsed = line.Split(',');

                        if (lineParsed.Length > 1)
                        {
                            result.Add(lineParsed[0].Trim(), lineParsed[1].Trim());
                        }
                    }
                }
            }

            return result;
        }
    }
}