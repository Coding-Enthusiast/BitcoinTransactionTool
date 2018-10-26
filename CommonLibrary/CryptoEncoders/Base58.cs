using CommonLibrary.Extensions;
using CommonLibrary.Hashing;
using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CommonLibrary.CryptoEncoders
{
    /// <summary>
    /// https://en.bitcoin.it/wiki/Base58Check_encoding
    /// </summary>
    public class Base58
    {
        public Base58()
        {
            b58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        }



        private const int CheckSumSize = 4;
        private readonly string b58Chars;



        /// <summary>
        /// Checks to see if a given string is a valid base-58 encoded string with a valid checksum.
        /// </summary>
        /// <param name="base58EncodedString">Input string to check.</param>
        /// <returns>True if input was a valid base-58 encoded string with checksum, false if otherwise.</returns>
        public bool IsValid(string base58EncodedString)
        {
            return HasValidChars(base58EncodedString) && HasValidCheckSum(base58EncodedString);
        }

        /// <summary>
        /// Checks to see if a given string has valid base-58 characters.
        /// <para/>* Doesn't verify checksum.
        /// </summary>
        /// <param name="base58EncodedString">Input string to check.</param>
        /// <returns>True if input was a valid base-58 encoded string (without verifying checksum).</returns>
        internal bool HasValidChars(string base58EncodedString)
        {
            if (base58EncodedString == null)
            {
                return false;
            }
            if (!base58EncodedString.All(x => b58Chars.Contains(x)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if a base-58 encoded string has a valid checksum.
        /// <para/> * Does not check validity of characters. Use <see cref="HasValidChars(string)"/> before calling this method.
        /// </summary>
        /// <param name="b58EncodedStringWithCheckSum">A valid base-58 encoded string.</param>
        /// <returns>True if input has a valid checksum, false otherwise. Throws an exception if given an invalid base-58 string</returns>
        internal bool HasValidCheckSum(string b58EncodedStringWithCheckSum)
        {
            byte[] data = DecodeWithoutValidation(b58EncodedStringWithCheckSum);
            if (data.Length < CheckSumSize)
            {
                return false;
            }
            byte[] dataWithoutCheckSum = data.SubArray(0, data.Length - CheckSumSize);
            byte[] checkSum = data.SubArrayFromEnd(CheckSumSize);
            byte[] calculatedCheckSum = CalculateCheckSum(dataWithoutCheckSum);

            return checkSum[0] == calculatedCheckSum[0]
                && checkSum[1] == calculatedCheckSum[1]
                && checkSum[2] == calculatedCheckSum[2]
                && checkSum[3] == calculatedCheckSum[3];
        }


        /// <summary>
        /// Converts a base-58 encoded string back to its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="b58EncodedString">Base-58 encoded string.</param>
        /// <returns>Byte array of the given string.</returns>
        public byte[] Decode(string b58EncodedString)
        {
            if (!HasValidChars(b58EncodedString))
            {
                throw new ArgumentException("Input is not a valid Base58 encoded string.");
            }

            return DecodeWithoutValidation(b58EncodedString);
        }


        /// <summary>
        /// Converts a valid base-58 encoded string back to its byte array representation.
        /// <para/> * Does not check validity of characters. Use <see cref="HasValidChars(string)"/> before calling this method.
        /// </summary>
        /// <remarks>
        /// By skipping validation (Linq.All) this method becomes 4 times faster.
        /// This is also 6 times faster than using BigInteger since it decodes directly to base-256.
        /// </remarks>
        /// <param name="validB58EncodedString">A valid base-58 encoded string.</param>
        /// <returns>Byte array of the given base-58 string.</returns>
        internal byte[] DecodeWithoutValidation(string validB58EncodedString)
        {
            int index = 0;
            int leadingZeroCount = 0;
            while (index < validB58EncodedString.Length && validB58EncodedString[index] == '1')
            {
                leadingZeroCount++;
                index++;
            }

            // Base-256 (byte array) in big-endian order
            byte[] b256 = new byte[(validB58EncodedString.Length - index) * 733 / 1000 + 1]; // log(58) / log(256), rounded up.
            for (; index < validB58EncodedString.Length; index++)
            {
                int carry = b58Chars.IndexOf(validB58EncodedString[index]);
                for (int i = b256.Length - 1; i >= 0; i--)
                {
                    carry += 58 * b256[i];
                    b256[i] = (byte)(carry % 256);
                    carry /= 256;
                }
            }

            // Skip leading zeroes in Base-256.
            int zeros = 0;
            while (zeros < b256.Length && b256[zeros] == 0)
            {
                zeros++;
            }

            byte[] result = new byte[leadingZeroCount + (b256.Length - zeros)];
            for (int i = leadingZeroCount; i < result.Length; i++)
            {
                result[i] = b256[zeros++];
            }
            return result;
        }


        /// <summary>
        /// Converts a base-58 encoded string back to its byte array representation while validating and removing checksum bytes.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="b58EncodedStringWithCheckSum">Base-58 encoded string with checksum.</param>
        /// <returns>Byte array of the given string.</returns>
        public byte[] DecodeWithCheckSum(string b58EncodedStringWithCheckSum)
        {
            if (!HasValidChars(b58EncodedStringWithCheckSum))
            {
                throw new ArgumentException("Input is not a valid base-58 encoded string.");
            }

            byte[] data = DecodeWithoutValidation(b58EncodedStringWithCheckSum);
            byte[] dataWithoutCheckSum = data.SubArray(0, data.Length - CheckSumSize);
            byte[] checkSum = data.SubArrayFromEnd(CheckSumSize);
            byte[] calculatedCheckSum = CalculateCheckSum(dataWithoutCheckSum);

            if (checkSum[0] != calculatedCheckSum[0] ||
                checkSum[1] != calculatedCheckSum[1] ||
                checkSum[2] != calculatedCheckSum[2] ||
                checkSum[3] != calculatedCheckSum[3])
            {
                throw new ArgumentException("Checksum of given data is incorect.");
            }

            return dataWithoutCheckSum;
        }


        /// <summary>
        /// Converts the given byte array to its equivalent string representation that is encoded with base-58 digits.
        /// </summary>
        /// <remarks>
        /// Unlike Decode functions, using BigInteger here makes things slightly faster. 
        /// The difference will be more noticeable with larger byte arrays such as extended keys (BIP32).
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">Byte array to encode.</param>
        /// <returns>The string representation in base-58.</returns>
        public string Encode(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Input can not be null!");
            }

            BigInteger big = data.ToBigInt(true, true);

            StringBuilder result = new StringBuilder();
            while (big > 0)
            {
                big = BigInteger.DivRem(big, 58, out BigInteger remainder);
                result.Insert(0, b58Chars[(int)remainder]);
            }

            // Append `1` for each leading 0 byte
            for (var i = 0; i < data.Length && data[i] == 0; i++)
            {
                result.Insert(0, '1');
            }

            return result.ToString();
        }


        /// <summary>
        /// Converts the given byte array to its equivalent string representation that is encoded with base-58 digits,
        /// with 4 byte appended checksum.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">Byte array to encode.</param>
        /// <returns>The string representation in base-58 with a checksum.</returns>
        public string EncodeWithCheckSum(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Cannot calculate checksum of null input!");
            }

            byte[] checkSum = CalculateCheckSum(data);
            return Encode(data.ConcatFast(checkSum));
        }


        /// <summary>
        /// Calculates checksum (first 4 bytes of 2x SHA256) of a given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">An array of bytes.</param>
        /// <returns>Checksum byte sequence (first 4 bytes of 2x SHA256).</returns>
        private byte[] CalculateCheckSum(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Cannot calculate checksum of null input!");
            }

            return HashFunctions.ComputeDoubleSha256(data).SubArray(0, CheckSumSize);
        }

    }
}
