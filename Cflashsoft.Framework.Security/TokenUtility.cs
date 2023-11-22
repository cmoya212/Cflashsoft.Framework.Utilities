using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens;

namespace Cflashsoft.Framework.Security
{
    /// <summary>
    /// Utility to create a self-expiring, short, encrypted string using Rijndael encryption suitable for HTTP headers and cookies.
    /// Author: C. Moya
    /// </summary>
    public static class TokenUtility
    {
        /// <summary>
        /// Create a short encrypted token using Rijndael with a randomized salt and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael with a randomized salt and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key)
        {
            return CreateToken(value, key, (DateTime?)null);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael with a randomized salt and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key, int expirationMinutes)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key, expirationMinutes);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael with a randomized salt and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key, int expirationMinutes)
        {
            return CreateToken(value, key, expirationMinutes > 0 ? new DateTime?(DateTime.Now.AddMinutes(expirationMinutes)) : null);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael with a randomized salt and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key, DateTime? expirationDate)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key, expirationDate);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael with a randomized salt and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key, DateTime? expirationDate)
        {
            byte[] salt = new byte[16];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(salt);
            }

            return CreateToken(value, key, salt, expirationDate);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key, string salt)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key, salt);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key, string salt)
        {
            return CreateToken(value, key, Encoding.ASCII.GetBytes(salt));
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key, byte[] salt)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key, salt);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key, byte[] salt)
        {
            return CreateToken(value, key, salt, (DateTime?)null);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key, string salt, int expirationMinutes)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key, salt, expirationMinutes);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key, string salt, int expirationMinutes)
        {
            return CreateToken(value, key, Encoding.ASCII.GetBytes(salt), expirationMinutes > 0 ? new DateTime?(DateTime.Now.AddMinutes(expirationMinutes)) : (DateTime?)null);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(string value, string key, byte[] salt, DateTime? expirationDate)
        {
            return CreateToken(Encoding.UTF8.GetBytes(value), key, salt, expirationDate);
        }

        /// <summary>
        /// Create a short encrypted token using Rijndael and optional self-expiration.
        /// </summary>
        public static string CreateToken(byte[] value, string key, byte[] salt, DateTime? expirationDate)
        {
            if (value == null)
                throw new ArgumentException("Input value cannot be null");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Encryption key must be specified");

            if (salt == null || salt.Length < 16)
                throw new ArgumentException("Salt value must be specified and be 16 bytes or larger");

            string result = null;

            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(key), salt);

                long expirationTicks = expirationDate.HasValue ? expirationDate.Value.ToUniversalTime().Ticks : 0;

                using (ICryptoTransform encryptor = rijndael.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(value, 0, value.Length);

                            if (expirationTicks > 0)
                            {
                                byte[] expirationBytes = BitConverter.GetBytes(expirationTicks);

                                cryptoStream.Write(expirationBytes, 0, expirationBytes.Length);
                                cryptoStream.WriteByte(1);
                            }
                            else
                            {
                                cryptoStream.WriteByte(0);
                            }

                            cryptoStream.FlushFinalBlock();

                            memoryStream.Write(salt, 0, salt.Length);

                            result = Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Decrypt a token. If the token has expired a SecurityTokenExpiredException will be thrown.
        /// </summary>
        public static string DecryptToken(string value, string key, bool ignoreDecryptErrors = false)
        {
            return DecryptToken(value, key, false, ignoreDecryptErrors);
        }

        /// <summary>
        /// Decrypt a token. If the token has expired a SecurityTokenExpiredException will be thrown.
        /// </summary>
        public static byte[] DecryptTokenAsBytes(string value, string key, bool ignoreDecryptErrors = false)
        {
            return DecryptTokenAsBytes(value, key, false, ignoreDecryptErrors);
        }

        /// <summary>
        /// Decrypt a token even if it has expired.
        /// </summary>
        public static string DecryptTokenUnsafe(string value, string key, bool ignoreDecryptErrors = false)
        {
            return DecryptToken(value, key, true, ignoreDecryptErrors);
        }

        /// <summary>
        /// Decrypt a token even if it has expired.
        /// </summary>
        public static byte[] DecryptTokenUnsafeAsBytes(string value, string key, bool ignoreDecryptErrors = false)
        {
            return DecryptTokenAsBytes(value, key, true, ignoreDecryptErrors);
        }

        internal static string DecryptToken(string value, string key, bool ignoreExpiration, bool ignoreDecryptErrors = false)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var result = DecryptTokenAsBytes(value, key, ignoreExpiration, ignoreDecryptErrors);

            if (result != null)
                return Encoding.UTF8.GetString(result);
            else
                return null;
        }

        internal static byte[] DecryptTokenAsBytes(string value, string key, bool ignoreExpiration, bool ignoreDecryptErrors)
        {
            byte[] result = null;

            try
            {
                using (RijndaelManaged rijndael = new RijndaelManaged())
                {
                    byte[] bytes = Convert.FromBase64String(value);
                    byte[] salt = new byte[16];

                    Array.Copy(bytes, bytes.Length - 16, salt, 0, 16);

                    PasswordDeriveBytes secretKey = new PasswordDeriveBytes(Encoding.ASCII.GetBytes(key), salt);

                    using (ICryptoTransform decryptor = rijndael.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(bytes, 0, bytes.Length - 16);

                                cryptoStream.FlushFinalBlock();

                                memoryStream.Position = memoryStream.Length - 1;

                                if (memoryStream.ReadByte() == 1)
                                {
                                    byte[] expirationBytes = new byte[8];

                                    memoryStream.Position = memoryStream.Length - 1 - expirationBytes.Length;

                                    memoryStream.Read(expirationBytes, 0, expirationBytes.Length);

                                    DateTime expirationDate = new DateTime(BitConverter.ToInt64(expirationBytes, 0));

                                    if (ignoreExpiration || expirationDate > DateTime.UtcNow)
                                    {
                                        result = DecodeResult(memoryStream, expirationBytes.Length);
                                    }
                                    else
                                    {
                                        throw new SecurityTokenExpiredException();
                                    }
                                }
                                else
                                {
                                    result = DecodeResult(memoryStream, 0);
                                }
                            }
                        }
                    }
                }
            }
            catch (SecurityTokenExpiredException)
            {
                if (!ignoreDecryptErrors)
                    throw;
            }
            catch (UnauthorizedAccessException)
            {
                if (!ignoreDecryptErrors)
                    throw;
            }
            catch (Exception ex)
            {
                if (!ignoreDecryptErrors)
                    throw new UnauthorizedAccessException("Error decrypting string. See inner exception for details.", ex);
            }

            return result;

            byte[] DecodeResult(MemoryStream stream, int offset)
            {
                stream.Position = 0;
                byte[] contents = new byte[(int)stream.Length - 1 - offset];
                stream.Read(contents, 0, contents.Length);
                return contents;
            }
        }

        /// <summary>
        /// Returns true if a token has expired.
        /// </summary>
        public static bool IsTokenExpired(string value, string key)
        {
            bool result = false;

            try
            {
                DecryptToken(value, key);
            }
            catch (SecurityTokenExpiredException)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Generate a unique key.
        /// </summary>
        public static string GenerateRandomKey()
        {
            byte[] bytes = new byte[10];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(bytes);
            }

            return Convert.ToBase64String(bytes);
        }
    }
}