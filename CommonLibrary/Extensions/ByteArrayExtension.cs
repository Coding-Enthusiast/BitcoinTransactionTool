using System;
using System.Numerics;

namespace CommonLibrary.Extensions
{
    public static class ByteArrayExtension
    {
        /// <summary>
        /// Appends a new byte to the beginning of the given byte array and returns a new array with a bigger length.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="arr">Source array to append to.</param>
        /// <param name="newItem">The value to append to source.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] AppendToBeginning(this byte[] arr, byte newItem)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr), "Byte array can not be null!");
            }

            byte[] result = new byte[arr.Length + 1];
            result[0] = newItem;
            Buffer.BlockCopy(arr, 0, result, 1, arr.Length);

            return result;
        }


        /// <summary>
        /// Appends a new byte to the end of the given byte array and returns a new array with a bigger length.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="arr">Source array to append to.</param>
        /// <param name="newItem">The value to append to source.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] AppendToEnd(this byte[] arr, byte newItem)
        {
            if (arr == null)
            {
                throw new ArgumentNullException(nameof(arr), "Byte array can not be null!");
            }

            byte[] result = new byte[arr.Length + 1];
            result[arr.Length] = newItem;
            Buffer.BlockCopy(arr, 0, result, 0, arr.Length);

            return result;
        }


        /// <summary>
        /// Concatinates two given byte arrays and returns a new byte array containing all the elements. 
        /// (~30 times faster than Linq)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="firstArray">irst set of bytes in the final array.</param>
        /// <param name="secondArray">Second set of bytes in the final array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ConcatFast(this byte[] firstArray, byte[] secondArray)
        {
            if (firstArray == null)
            {
                throw new ArgumentNullException(nameof(firstArray), "First array can not be null!");
            }
            if (secondArray == null)
            {
                throw new ArgumentNullException(nameof(secondArray), "Second array can not be null!");
            }

            byte[] result = new byte[firstArray.Length + secondArray.Length];
            Buffer.BlockCopy(firstArray, 0, result, 0, firstArray.Length);
            Buffer.BlockCopy(secondArray, 0, result, firstArray.Length, secondArray.Length);

            return result;
        }


        /// <summary>
        /// Creates a new array from the given array by taking a specified number of items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index, int count)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException(nameof(sourceArray), $"{nameof(sourceArray)} can not be null!");
            }
            if (index < 0 || index > sourceArray.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index can't be negative or bigger than {sourceArray.Length - 1}");
            }
            if (count > sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(sourceArray)} is not long enough.");
            }

            byte[] result = new byte[count];
            Buffer.BlockCopy(sourceArray, index, result, 0, count);

            return result;
        }


        /// <summary>
        /// Creates a new array from the given array by taking items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index)
        {
            return SubArray(sourceArray, index, sourceArray.Length - index);
        }


        /// <summary>
        /// Creates a new array from the given array by taking items from the end of the array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArrayFromEnd(this byte[] sourceArray, int count)
        {
            if (sourceArray == null)
            {
                throw new ArgumentNullException(nameof(sourceArray), $"{nameof(sourceArray)} can not be null!");
            }
            if (count > sourceArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(sourceArray)} is not long enough.");
            }

            return sourceArray.SubArray(sourceArray.Length - count, count);
        }


        /// <summary>
        /// Converts a byte array to base-16 (Hexadecimal) encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <returns>Base-16 (Hexadecimal) encoded string.</returns>
        public static string ToBase16(this byte[] ba)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }

            char[] ca = new char[ba.Length * 2];
            int b;
            for (int i = 0; i < ba.Length; i++)
            {
                b = ba[i] >> 4;
                ca[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = ba[i] & 0xF;
                ca[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(ca);
        }


        /// <summary>
        /// Converts a byte array to base-64 encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <returns>Base-64 encoded string.</returns>
        public static string ToBase64(this byte[] ba)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }

            return Convert.ToBase64String(ba);
        }


        /// <summary>
        /// Returns a 16-bit (2 byte) unsigned integer converted from given two bytes.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must contain 2 bytes).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 16-bit unsigned integer.</returns>
        public static ushort ToUInt16(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }
            if (ba.Length != sizeof(ushort))
            {
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 2 bytes.");
            }

            ushort result = 0;
            if (isBigEndian)
            {
                for (int i = 0, j = sizeof(ushort) - 1; i < sizeof(ushort); i++, j--)
                {
                    result |= (ushort)(ba[i] << (ushort)(8 * j));
                }
            }
            else
            {
                for (int i = 0; i < sizeof(ushort); i++)
                {
                    result |= (ushort)(ba[i] << (8 * i));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a 32-bit (4 byte) unsigned integer converted from given four bytes.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must contain 4 bytes).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 32-bit unsigned integer.</returns>
        public static uint ToUInt32(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }
            if (ba.Length != sizeof(uint))
            {
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 4 bytes.");
            }

            uint result = 0;
            if (isBigEndian)
            {
                for (int i = 0, j = sizeof(uint) - 1; i < sizeof(uint); i++, j--)
                {
                    result |= ((uint)ba[i] << (8 * j));
                }
            }
            else
            {
                for (int i = 0; i < sizeof(uint); i++)
                {
                    result |= ((uint)ba[i] << (8 * i));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a 64-bit (8 byte) unsigned integer converted from given eight bytes
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must contain 8 bytes).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 64-bit unsigned integer.</returns>
        public static ulong ToUInt64(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }
            if (ba.Length != sizeof(ulong))
            {
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 8 bytes.");
            }

            ulong result = 0;
            if (isBigEndian)
            {
                for (int i = 0, j = sizeof(ulong) - 1; i < sizeof(ulong); i++, j--)
                {
                    result |= ((ulong)ba[i] << (8 * j));
                }
            }
            else
            {
                for (int i = 0; i < sizeof(ulong); i++)
                {
                    result |= ((ulong)ba[i] << (8 * i));
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the <see cref="BigInteger"/> equivalant of the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <param name="treatAsPositive"></param>
        /// <returns>A BigInteger.</returns>
        public static BigInteger ToBigInt(this byte[] ba, bool isBigEndian, bool treatAsPositive)
        {
            if (ba == null)
            {
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            }

            if (ba.Length == 0)
            {
                return BigInteger.Zero;
            }

            // Make a copy of the array to avoid changing original array in case a reverse was needed.
            byte[] bytesToUse = new byte[ba.Length];
            Buffer.BlockCopy(ba, 0, bytesToUse, 0, ba.Length);

            // BigInteger constructor takes little-endian bytes
            if (isBigEndian)
            {
                Array.Reverse(bytesToUse);
            }

            if (treatAsPositive && (bytesToUse[bytesToUse.Length - 1] & 0x80) > 0)
            {
                bytesToUse = bytesToUse.AppendToEnd(0);
            }

            return new BigInteger(bytesToUse);
        }

    }
}
