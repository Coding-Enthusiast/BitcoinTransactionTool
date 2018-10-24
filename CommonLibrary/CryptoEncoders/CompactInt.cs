using CommonLibrary.Extensions;
using System;

namespace CommonLibrary.CryptoEncoders
{
    /// <summary>
    /// https://bitcoin.org/en/developer-reference#compactsize-unsigned-integers
    /// </summary>
    public class CompactInt
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> to the value zero.
        /// </summary>
        public CompactInt()
        {
            Bytes = new byte[] { 0 };
            Number = 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> to the value indicated by a 64-bit unsigned integer.
        /// </summary>
        /// <param name="number">A 64-bit unsigned integer.</param>
        public CompactInt(ulong number)
        {
            Number = number;
            Bytes = Write(number);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> to the value indicated by an array of bytes.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">An array of bytes.</param>
        public CompactInt(byte[] ba)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Input byte array can not be null!");
            }

            int size = 0;
            Number = Read(ba, ref size);
            Bytes = ba.SubArray(0, size);
        }



        /// <summary>
        /// Maximum size of Compact Size Int. 1 byte followed by UInt64 (9 byte total).
        /// </summary>
        public const int MaxSize = 1 + sizeof(ulong);

        /// <summary>
        /// Array of bytes representing the CompactInt
        /// </summary>
        public byte[] Bytes { get; set; }

        /// <summary>
        /// Number representing the CompactInt
        /// </summary>
        public ulong Number { get; set; }



        /// <summary>
        /// Checks to see if the given byte array starts with a valid Compact Size Integer.
        /// <para/> byte array should be little-endian
        /// </summary>
        /// <param name="cInt">Byte array starting with a Compact Size Integer in little endian order.</param>
        /// <returns>True if the starting bytes are a valid Compact Size Integer, false if otherwise.</returns>
        public static bool StartsWithValidCompactInt(byte[] cInt)
        {
            if (cInt == null)
            {
                return false;
            }

            int size1 = 1 + sizeof(ushort);
            int size2 = 1 + sizeof(uint);
            int size3 = 1 + sizeof(ulong);

            return (cInt[0] <= 252)
                || (cInt[0] == 253 && cInt.Length >= size1 && cInt.SubArray(1, size1 - 1).ToUInt16(false) >= 253)
                || (cInt[0] == 254 && cInt.Length >= size2 && cInt.SubArray(1, size2 - 1).ToUInt32(false) > ushort.MaxValue)
                || (cInt[0] == 255 && cInt.Length >= size3 && cInt.SubArray(1, size3 - 1).ToUInt64(false) > uint.MaxValue);
        }


        /// <summary>
        /// Reads a Compact Size Int. also returns size of the data that was used inside the byte array. 
        /// <para/> byte array is little-endian
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="data">Byte arrady starting with a Compact Size Int to read.</param>
        /// <param name="size">Size of the Compact Size Int inside given <paramref name="data"/>.</param>
        /// <returns>Unsigned integer equal to the Compact Size Int.</returns>
        public static ulong Read(byte[] data, ref int size)
        {
            if (!StartsWithValidCompactInt(data))
            {
                throw new ArgumentException("Input is not a valid Compact Size Int.", nameof(data));
            }

            ulong result = 0;
            if (data[0] <= 252)
            {
                result = data[0];
                size = sizeof(byte);
            }
            else if (data[0] == 0xfd)
            {
                result = data.SubArray(1, sizeof(ushort)).ToUInt16(false);
                size = 1 + sizeof(ushort);
            }
            else if (data[0] == 0xfe)
            {
                result = data.SubArray(1, sizeof(uint)).ToUInt32(false);
                size = 1 + sizeof(uint);
            }
            else if (data[0] == 0xff)
            {
                result = data.SubArray(1, sizeof(ulong)).ToUInt64(false);
                size = 1 + sizeof(ulong);
            }

            return result;
        }


        /// <summary>
        /// Makes a variable length integer aka Compact Size Int.
        /// <para/> byte array is little-endian
        /// </summary>
        /// <param name="num">Number to convert to Compact Size Int.</param>
        /// <returns>Byte array of the Compact Size Int result in little endian order.</returns>
        public static byte[] Write(ulong num)
        {
            byte[] result;
            if (num <= 252) // 1 Byte
            {
                result = new byte[] { (byte)num };
            }
            else if (num <= 0xffff) // 1 + 2 Byte
            {
                result = new byte[] { 0xfd }.ConcatFast(((ushort)num).ToByteArray(false));
            }
            else if (num <= 0xffffffff) // 1 + 4 Byte
            {
                result = new byte[] { 0xfe }.ConcatFast(((uint)num).ToByteArray(false));
            }
            else // < 0xffffffffffffffff // 1 + 8 Byte
            {
                result = new byte[] { 0xff }.ConcatFast(((ulong)num).ToByteArray(false));
            }

            return result;
        }

    }
}
