using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CommonLibrary
{
    /// <summary>
    /// https://en.bitcoin.it/wiki/Technical_background_of_version_1_Bitcoin_addresses
    /// </summary>
    public class BitcoinConversions
    {
        /// <summary>
        /// Satohi is the smallest amount of bitcoin, 10^-8
        /// </summary>
        public const decimal Satoshi = 0.00000001m;

        /// <summary>
        /// Characters used in Base58Encoding which is all chars excluding "0OIl" 
        /// </summary>
        private const string Base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";


        /// <summary>
        /// Converts a hex string to intiger.
        /// </summary>
        /// <param name="hex">hex value to convert.</param>
        /// <returns></returns>
        //public static UInt32 HexToInt(string hex)
        //{
        //    byte[] ba = HexToByteArray(hex);
        //    if (!BitConverter.IsLittleEndian)
        //    {
        //        ba = ba.Reverse().ToArray();
        //    }
        //    // Prevent out of range exception by adding zeros.
        //    if (ba.Length < 4)
        //    {
        //        ba = ba.Concat(new byte[4 - ba.Length]).ToArray();
        //    }
        //    return BitConverter.ToUInt32(ba, 0);
        //}


        /// <summary>
        /// Converts a hex string to bye array.
        /// </summary>
        /// <param name="hex">hex to convert.</param>
        /// <returns>byte array of hex</returns>
        public static byte[] HexToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        /// <summary>
        /// Converts a byte array to hex string.
        /// </summary>
        /// <param name="b">byte array to convert.</param>
        /// <returns></returns>
        public static string ByteArrayToHex(byte[] b)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                result.Append(b[i].ToString("x2"));
            }
            return result.ToString();
        }


        /// <summary>
        /// Converts the Hash160 (RIPEMD-160) to Base58 encoded string.
        /// </summary>
        /// <param name="data">Hash160 string value.</param>
        /// <returns>Base58 Encoded result.</returns>
        public static string Base58ToHash160(string data)
        {
            // Decode Base58 string to BigInteger 
            BigInteger intData = 0;
            for (var i = 0; i < data.Length; i++)
            {
                intData = intData * 58 + Base58Chars.IndexOf(data[i]);
            }

            // Encode BigInteger to byte[]
            // Leading zero bytes get encoded as leading `1` characters
            var bData = intData.ToByteArray();
            if (BitConverter.IsLittleEndian)
            {
                bData = bData.Reverse().ToArray();
            }
            bData = bData.SkipWhile(b => b == 0).ToArray();
            var hash160 = BitConverter.ToString(bData).Replace("-", "").ToLower();
            // Remove checksum (4 bytes = 8 last chars)
            return hash160.Remove(hash160.Length - 8);
        }


        /// <summary>
        /// Computes the hash value of the specified byte array.
        /// </summary>
        /// <param name="b">byte array to compute hash of.</param>
        /// <returns></returns>
        private static byte[] DoubleShaByte(byte[] b)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                byte[] hash1 = sha.ComputeHash(b);
                byte[] hash2 = sha.ComputeHash(hash1);

                return hash2;
            }
        }

        /// <summary>
        /// Returns Transaction Id for the given Raw Transaction hex.
        /// </summary>
        /// <param name="txHex">Raw Transaction hex.</param>
        /// <returns>Transaction Id</returns>
        public static string GetTxId(string txHex)
        {
            byte[] ba = HexToByteArray(txHex);
            byte[] sha2x = DoubleShaByte(ba);

            return ByteArrayToHex(sha2x.Reverse().ToArray());
        }


        /// <summary>
        /// Converts Public Key to Hash160 (RIPEMD-160)
        /// <para/>A.K.A. payload
        /// </summary>
        /// <param name="pubKey">Bitcoin Public Key</param>
        /// <returns>hash160 (RIPEMD-160)</returns>
        public static byte[] PubKeyToHash160(string pubKey)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                byte[] pubKeyBytes = HexToByteArray(pubKey);

                // 2. Perform Sha256 hashing on public key
                byte[] hash1 = sha.ComputeHash(pubKeyBytes);

                // 3. Perform RIPEMD-160 hashing on step 2
                using (RIPEMD160 r160 = new RIPEMD160Managed())
                {
                    byte[] hash2 = r160.ComputeHash(hash1);

                    return hash2;
                }
            }
        }

        /// <summary>
        /// Converts Hash160 (RIPEMD-160) to Base58 bitcoin Address.
        /// </summary>
        /// <param name="hash160">Hash160 (RIPEMD-160) bytes</param>
        /// <returns>Base58 encoded bytes</returns>
        public static string Hash160ToBase58(byte[] hash160)
        {
            // 4. Add version byte in front of RIPEMD-160 hash (0x00 for Main Network)
            byte[] ver = { 0x00 };
            byte[] hash160Extended = ver.Concat(hash160).ToArray();

            // 5&6. Perform SHA-256 hash on the extended RIPEMD-160 result x2
            byte[] sha2x = DoubleShaByte(hash160Extended);

            // 7. The first 4 bytes are address checksum
            byte[] checkSumByte = new byte[4];
            checkSumByte[0] = sha2x[0];
            checkSumByte[1] = sha2x[1];
            checkSumByte[2] = sha2x[2];
            checkSumByte[3] = sha2x[3];

            // 8. 25-byte binary Bitcoin Address = RIPEMD-160 extended + Checksum
            byte[] hash160WithCheckSum = hash160Extended.Concat(checkSumByte).ToArray();

            return Hash160WithCheckSumToBase58(hash160WithCheckSum);
        }
        public static string Hash160ToBase58(string hash160)
        {
            byte[] hash160Bytes = HexToByteArray(hash160);
            return Hash160ToBase58(hash160Bytes);
        }

        /// <summary>
        /// Converts hash to Base58 Encoded string.
        /// </summary>
        /// <param name="hash">1-byte_version + hash_or_other_data + 4-byte_check_code</param>
        /// <returns>Base58 encoded result</returns>
        public static string Hash160WithCheckSumToBase58(byte[] hash)
        {
            // Decode byte[] to BigInteger
            BigInteger intData = hash.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t);

            // Encode BigInteger to Base58 string
            StringBuilder result = new StringBuilder();
            while (intData > 0)
            {
                var remainder = (int)(intData % 58);
                intData /= 58;
                result.Insert(0, Base58Chars[remainder]);
            }

            // Append '1' for each leading 0 byte
            for (var i = 0; i < hash.Length && hash[i] == 0; i++)
            {
                result.Insert(0, '1');
            }

            return result.ToString();
        }
        public static string Hash160WithCheckSumToBase58(string hash)
        {
            byte[] hashByte = HexToByteArray(hash);
            return Hash160WithCheckSumToBase58(hashByte);
        }

        /// <summary>
        /// Converts Public Key to Base58 encoded Bitcoin Address.
        /// </summary>
        /// <param name="pubKey">Bitcoin Public Key.</param>
        /// <returns>Base58 encoded Bitcoin Address.</returns>
        public static string PubKeyToBase58(string pubKey)
        {
            byte[] hash160 = PubKeyToHash160(pubKey);

            string base58 = Hash160ToBase58(hash160);

            return base58;
        }
    }
}
