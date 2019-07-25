using System;
using System.Collections.Generic;
using System.Text;

namespace Cflashsoft.Framework.Security
{
    /// <summary>
    /// The exception that is thrown when a security token that has an expiration time in the past is received.
    /// </summary>
    public class TokenExpiredException : Exception
    {
    }
}
