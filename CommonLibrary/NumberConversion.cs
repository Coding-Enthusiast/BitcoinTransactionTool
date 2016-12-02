using System;
using System.Linq;
using System.Text;

namespace CommonLibrary
{
    public class NumberConversions
    {
        /// <summary>
        /// Converts an intiger to hex string.
        /// </summary>
        /// <param name="iValue">Integer to convert.</param>
        /// <param name="bLength">Length of the hex in bytes.</param>
        /// <returns>Hex string</returns>
        public static string IntToHex(UInt64 iValue, int bLength)
        {
            byte[] b = BitConverter.GetBytes(iValue);
            // Prevent out of range exception by adding zeros.
            if (b.Length < bLength)
            {
                b = b.Concat(new byte[bLength - b.Length]).ToArray();
            }
            // Make sure byte array is BigEndian
            if (!BitConverter.IsLittleEndian)
            {
                b = b.Reverse().ToArray();
            }
            StringBuilder hex = new StringBuilder();
            for (int i = 0; i < bLength; i++)
            {
                hex.Append(b[i].ToString("x2"));
            }
            return hex.ToString();
        }


        /// <summary>
        /// Converts a hex string to byte array.
        /// </summary>
        /// <param name="hex">hex to convert.</param>
        /// <returns>Byte array of hex</returns>
        public static byte[] HexToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }


        /// <summary>
        /// Converts a hex string to unsigned intiger.
        /// </summary>
        /// <param name="hex">hex value to convert.</param>
        /// <returns>Unsigned Long, result can be smaller and can be cast into uint32,...</returns>
        public static UInt64 HexToUInt(string hex)
        {
            byte[] ba = HexToByteArray(hex);
            if (!BitConverter.IsLittleEndian)
            {
                ba = ba.Reverse().ToArray();
            }
            // Prevent out of range exception by adding zeros.
            if (ba.Length < 8)
            {
                ba = ba.Concat(new byte[8 - ba.Length]).ToArray();
            }

            return BitConverter.ToUInt64(ba, 0);
        }


        /// <summary>
        /// Makes a compacts size Unsigned Int64.
        /// <para/> https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers
        /// </summary>
        /// </summary>
        /// <param name="num">Number to convert to Compact Size string.</param>
        /// <returns></returns>
        public static string MakeCompactSize(UInt64 num)
        {
            string result = string.Empty;
            if (num <= 252) // 1 Byte
            {
                result = IntToHex(num, 1);
            }
            else if (num <= 0xffff) // 1 + 2 Byte
            {
                result = "fd" + IntToHex(num, 2);
            }
            else if (num <= 0xffffffff) // 1 + 4 Byte
            {
                result = "fe" + IntToHex(num, 4);
            }
            else // < 0xffffffffffffffff // 1 + 8 Byte
            {
                result = "ff" + IntToHex(num, 8);
            }

            return result;
        }


        /// <summary>
        /// Reads a compacts size hex and moves index forward.
        /// <para/> https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers
        /// </summary>
        /// <param name="hex">Transaction hex containing compactSize UInt.</param>
        /// <param name="i">Index of comactSize UInt inside of hex.</param>
        /// <returns></returns>
        public static UInt64 ReadCompactSize(string hex, ref int i)
        {
            string firstByte = hex.Substring(i, 2);
            string remainingBytes;
            byte firstInt = (byte)HexToUInt(firstByte);
            UInt64 result = 0;
            if (firstInt <= 252) // UInt8 (1 byte)
            {
                result = HexToUInt(firstByte);
                i = i + 2;
            }
            else if (firstInt == 253) // 0xfd followed by the number as UInt16 (2 byte)
            {
                remainingBytes = hex.Substring(i + 2, 4);
                result = HexToUInt(remainingBytes);
                i = i + 2 + 4;
            }
            else if (firstInt == 254) // 0xfe followed by the number as UInt32 (4 byte)
            {
                remainingBytes = hex.Substring(i + 2, 8);
                result = HexToUInt(remainingBytes);
                i = i + 2 + 8;
            }
            else if (firstInt == 255) // 0xff followed by the number as UInt64 (8 byte)
            {
                remainingBytes = hex.Substring(i + 2, 16);
                result = HexToUInt(remainingBytes);
                i = i + 2 + 16;
            }

            return result;
        }
    }
}
