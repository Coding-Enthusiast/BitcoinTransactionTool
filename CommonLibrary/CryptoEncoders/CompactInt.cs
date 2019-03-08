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
        /// Initializes a new instance of <see cref="CompactInt"/> to the value indicated by a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="value">Value to use (must be positive).</param>
        public CompactInt(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Length can not be negative!");

            SetFields((ulong)value);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> to the value indicated by a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer to use.</param>
        public CompactInt(uint value) : this((ulong)value)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> to the value indicated by a 64-bit unsigned integer.
        /// </summary>
        /// <param name="number">The 64-bit unsigned integer to use.</param>
        public CompactInt(ulong number)
        {
            SetFields(number);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompactInt"/> to the value indicated by an array of bytes.
        /// The byte array can contain extra bytes at the end which will be ignored.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="data">An array of bytes in little-endian order containing a <see cref="CompactInt"/>.</param>
        public CompactInt(byte[] data)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Input byte array can not be null!");


            if (data[0] <= 252)
            {
                Number = data[0];
                Bytes = new byte[] { data[0] };
            }
            else if (data[0] == 253 && data.Length >= (1 + sizeof(ushort))) //0xfd --> must be 2 bytes
            {
                Number = data.SubArray(1, sizeof(ushort)).ToUInt16(false);
                if (Number <= 252)
                {
                    throw new FormatException("For values less than 253, one byte should be used.");
                }
                Bytes = data.SubArray(0, 1 + sizeof(ushort));
            }
            else if (data[0] == 254 && data.Length >= (1 + sizeof(uint))) //0xfe --> must be 4 bytes
            {
                Number = data.SubArray(1, sizeof(uint)).ToUInt32(false);
                if (Number <= ushort.MaxValue)
                {
                    throw new FormatException($"For values less than {ushort.MaxValue}, two byte should be used.");
                }
                Bytes = data.SubArray(0, 1 + sizeof(uint));
            }
            else if (data[0] == 255 && data.Length >= (1 + sizeof(ulong))) //0xff --> must be 8 bytes
            {
                Number = data.SubArray(1, sizeof(ulong)).ToUInt64(false);
                if (Number <= uint.MaxValue)
                {
                    throw new FormatException($"For values less than {uint.MaxValue}, four byte should be used.");
                }
                Bytes = data.SubArray(0, 1 + sizeof(ulong));
            }
            else
            {
                throw new FormatException("Invalid compact int format.");
            }
        }



        /// <summary>
        /// Maximum size of Compact Size Int. 1 byte followed by UInt64 (9 byte total).
        /// </summary>
        public const int MaxSize = 1 + sizeof(ulong);

        /// <summary>
        /// Minimum size of Compact Size Int. 1 byte.
        /// </summary>
        public const int MinSize = 1;

        /// <summary>
        /// Array of bytes representing the CompactInt
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Number representing the CompactInt
        /// </summary>
        public ulong Number { get; private set; }



        private void SetFields(ulong num)
        {
            Number = num;
            if (num <= 252) // 1 Byte
            {
                Bytes = new byte[] { (byte)num };
            }
            else if (num <= 0xffff) // 1 + 2 Byte
            {
                Bytes = new byte[] { 0xfd }.ConcatFast(((ushort)num).ToByteArray(false));
            }
            else if (num <= 0xffffffff) // 1 + 4 Byte
            {
                Bytes = new byte[] { 0xfe }.ConcatFast(((uint)num).ToByteArray(false));
            }
            else // < 0xffffffffffffffff // 1 + 8 Byte
            {
                Bytes = new byte[] { 0xff }.ConcatFast(((ulong)num).ToByteArray(false));
            }
        }



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

    }
}
