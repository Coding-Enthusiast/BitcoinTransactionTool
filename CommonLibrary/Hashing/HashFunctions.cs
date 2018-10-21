using System;
using System.Security.Cryptography;

namespace CommonLibrary.Hashing
{
    public class HashFunctions
    {
        /// <summary>
        /// Computes SHA256 hash of the given byte array *twice*.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to perform double SHA256 on.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ComputeDoubleSha256(byte[] ba)
        {
            return ComputeSha256(ComputeSha256(ba));
        }

        /// <summary>
        /// Computes SHA256 hash of the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to perform SHA256 on.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ComputeSha256(byte[] ba)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }

            using (SHA256 sha = new SHA256Managed())
            {
                return sha.ComputeHash(ba);
            }
        }

        /// <summary>
        /// Computes RIPEMD160 hash of the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to perform RIPEMD160 on.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ComputeRipemd160(byte[] ba)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }

            using (RIPEMD160 rp = new RIPEMD160Managed())
            {
                return rp.ComputeHash(ba);
            }
        }

        /// <summary>
        /// Computes RIPEMD160 hash of SHA256 hash of the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to perform RIPEMD160 after SHA256 on.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ComputeHash160(byte[] ba)
        {
            return ComputeRipemd160(ComputeSha256(ba));
        }


        /// <summary>
        /// Computes HMACSHA512 hash of a given byte array with the specified key data.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to perform HMACSHA512 on.</param>
        /// <param name="key">The secret key for HMACSHA512 encryption. Recommended size is 128 bytes. 
        /// If it is smaller it will be padded to reach 128 and if it is bigger it will be hashed with SHA512 to reach 128 bytes
        /// </param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ComputeHmacSha512(byte[] ba, byte[] key)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Key can not be null.");
            }

            using (HMACSHA512 hm = new HMACSHA512(key))
            {
                return hm.ComputeHash(ba);
            }
        }

    }
}
