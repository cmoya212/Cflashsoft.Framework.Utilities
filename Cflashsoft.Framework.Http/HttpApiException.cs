﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace Cflashsoft.Framework.Http
{
    /// <summary>
    /// The exception that is thrown when an HTTP request is unsuccessful.
    /// </summary>
    public class HttpApiException : HttpRequestException
    {
        /// <summary>
        /// The HTTP status code of the response.
        /// </summary>
        public int? StatusCode { get; private set; }

        /// <summary>
        /// The content of the response represented as a string.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the HttpApiException class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="content">The string content of the response.</param>
        public HttpApiException(int statusCode = 500, string content = null)
            :base()
        {
            this.StatusCode = statusCode;
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the HttpApiException class.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="content">The string content of the response.</param>
        public HttpApiException(string message, int statusCode = 500, string content = null)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the HttpApiException class.
        /// </summary>
        /// <param name="message">A message that describes the current exception.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="content">The string content of the response.</param>
        /// <param name="innerException">The inner exception.</param>
        public HttpApiException(string message, Exception innerException, int statusCode = 500, string content = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.Content = content;
        }
    }
}
