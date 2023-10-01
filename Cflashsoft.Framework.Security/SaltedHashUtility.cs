using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cflashsoft.Framework.Security
{
    /// <summary>
    /// Utility to hash passwords with a random salt and store the salt along with the hash in the same structure.
    /// Author: C. Moya
    /// </summary>
    public static class SaltedHashUtility
    {
        /// <summary>
        /// Hash provider.
        /// </summary>
        public enum Provider
        {
            /// <summary>
            /// 16 byte MD5 hash.
            /// </summary>
            MD5Cng = 0,
            /// <summary>
            /// 32 byte SHA256 hash.
            /// </summary>
            SHA256Managed = 1
        }

        /// <summary>
        /// Computes a hash and stores the salt as part of the result.
        /// The result is a 32 byte array with 16 bytes for the hash and 16 bytes for the salt and can be stored
        /// in the database in a single 32-byte binary column.
        /// </summary>
        /// <param name="value">The input to compute the hash code for.</param>
        /// <param name="saltLength">The length of the salt to be used. Minimum is 16 bytes.</param>
        /// <returns></returns>
        public static byte[] ComputeHash(string value, int saltLength)
        {
            return ComputeHash(value, Provider.MD5Cng, saltLength);
        }


        /// <summary>
        /// Computes a hash and stores the salt as part of the result.
        /// The typical result is a 32 byte array with 16 bytes for the hash (32 for SHA256) and 16 bytes for the salt and can be stored
        /// in the database in a single 32-byte binary column (48 for SHA256) depending on the provider specified.
        /// </summary>
        /// <param name="value">The input to compute the hash code for.</param>
        /// <param name="provider">Provider such as MD5 or SHA256.</param>
        /// <param name="saltLength">The length of the salt to be used. Minimum is 16 bytes.</param>
        /// <returns>The computed hash code along with the salt.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static byte[] ComputeHash(string value, Provider provider, int saltLength)
        {
            if (saltLength < 16)
                throw new ArgumentOutOfRangeException("Salt length cannot be less than 16.");

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty.");

            byte[] result = null;
            byte[] salt = new byte[saltLength];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetNonZeroBytes(salt);
            }

            byte[] contentValuePart = Encoding.UTF8.GetBytes(value);
            byte[] content = new byte[contentValuePart.Length + saltLength];

            Array.Copy(contentValuePart, 0, content, 0, contentValuePart.Length);
            Array.Copy(salt, 0, content, contentValuePart.Length, saltLength);

            result = new byte[saltLength + GetHashLength(provider)];

            Array.Copy(salt, 0, result, 0, saltLength);

            byte[] passwordHash = null;

            switch (provider)
            {
                case Provider.MD5Cng:
                    using (MD5Cng md5 = new MD5Cng())
                    {
                        passwordHash = md5.ComputeHash(content);
                    }
                    break;
                case Provider.SHA256Managed:
                    using (SHA256Managed sha256 = new SHA256Managed())
                    {
                        passwordHash = sha256.ComputeHash(content);
                    }
                    break;
                default:
                    throw new NotSupportedException("Specified provider is not supported.");
            }

            Array.Copy(passwordHash, 0, result, saltLength, passwordHash.Length);

            return result;
        }


        /// <summary>
        /// Compare a value to a hash with the salt stored as part of the hash itself.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="saltedHash">The hash with salt.</param>
        /// <param name="saltLength">The length of the salt to be used. Minimum is 16 bytes.</param>
        /// <returns>Returns true if the comparison is successful.</returns>
        public static bool HashEqual(string value, byte[] saltedHash, int saltLength)
        {
            return HashEqual(value, saltedHash, Provider.MD5Cng, saltLength);
        }

        /// <summary>
        /// Compare a value to a hash with the salt stored as part of the hash itself.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="saltedHash">The hash with salt.</param>
        /// <param name="provider">Provider such as MD5 or SHA256.</param>
        /// <param name="saltLength">The length of the salt to be used. Minimum is 16 bytes.</param>
        /// <returns>Returns true if the comparison is successful.</returns>
        public static bool HashEqual(string value, byte[] saltedHash, Provider provider, int saltLength)
        {
            return HashEqual(Encoding.UTF8.GetBytes(value), saltedHash, provider, saltLength);
        }

        /// <summary>
        /// Compare a value to a hash with the salt stored as part of the hash itself.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="saltedHash">The hash with salt.</param>
        /// <param name="saltLength">The length of the salt to be used. Minimum is 16 bytes.</param>
        /// <returns>Returns true if the comparison is successful.</returns>
        public static bool HashEqual(byte[] value, byte[] saltedHash, int saltLength)
        {
            return HashEqual(value, saltedHash, Provider.MD5Cng, saltLength);
        }

        /// <summary>
        /// Compare a value to a hash with the salt stored as part of the hash itself.
        /// </summary>
        /// <param name="value">The value to compare.</param>
        /// <param name="saltedHash">The hash with salt.</param>
        /// <param name="provider">Provider such as MD5 or SHA256.</param>
        /// <param name="saltLength">The length of the salt to be used. Minimum is 16 bytes.</param>
        /// <returns>Returns true if the comparison is successful.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public static bool HashEqual(byte[] value, byte[] saltedHash, Provider provider, int saltLength)
        {
            if (saltLength < 16)
                throw new ArgumentOutOfRangeException("Salt length cannot be less than 16.");

            bool result = false;

            int hashLength = saltedHash.Length - saltLength;

            byte[] salt = new byte[saltLength];

            Array.Copy(saltedHash, 0, salt, 0, saltLength);

            byte[] hashPart = new byte[hashLength];

            Array.Copy(saltedHash, saltLength, hashPart, 0, hashLength);

            byte[] content = new byte[value.Length + saltLength];

            Array.Copy(value, 0, content, 0, value.Length);
            Array.Copy(salt, 0, content, value.Length, saltLength);

            byte[] valueHashPart = null;

            switch (provider)
            {
                case Provider.MD5Cng:
                    using (MD5Cng md5 = new MD5Cng())
                    {
                        valueHashPart = md5.ComputeHash(content);
                    }
                    break;
                case Provider.SHA256Managed:
                    using (SHA256Managed sha256 = new SHA256Managed())
                    {
                        valueHashPart = sha256.ComputeHash(content);
                    }
                    break;
                default:
                    throw new NotSupportedException("Specified provider is not supported.");
            }

            result = valueHashPart.SequenceEqual(hashPart);

            return result;
        }

        /// <summary>
        /// Returns the byte length of the hash of the specified provider.
        /// </summary>
        /// <param name="provider">Provider such as MD5 or SHA256.</param>
        /// <returns>The byte length of the hash.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static int GetHashLength(Provider provider)
        {
            switch (provider)
            {
                case Provider.MD5Cng:
                    return 16;
                case Provider.SHA256Managed:
                    return 32;
                default:
                    throw new NotSupportedException("Specified provider is not supported.");
            }
        }
    }
}
