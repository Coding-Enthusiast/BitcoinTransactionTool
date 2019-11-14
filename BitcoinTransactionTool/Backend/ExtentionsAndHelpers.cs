// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace BitcoinTransactionTool.Backend
{
    /// <summary>
    /// Helper class for working with byte arrays
    /// </summary>
    public class ByteArray
    {
        /// <summary>
        /// Concatinates a list of arrays together and returns a bigger array containing all the elements.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="arrays">Array of byte arrays to concatinate.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ConcatArrays(params byte[][] arrays)
        {
            if (arrays == null)
                throw new ArgumentNullException(nameof(arrays), "Array params can not be null.");

            // Linq is avoided to increase speed.
            int len = 0;
            foreach (var arr in arrays)
            {
                if (arr == null)
                {
                    throw new ArgumentNullException(nameof(arr), "Can't concatinate with null array(s)!");
                }
                len += arr.Length;
            }

            byte[] result = new byte[len];

            int offset = 0;
            foreach (var arr in arrays)
            {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return result;
        }

    }



    public static class ByteArrayExtension
    {
        /// <summary>
        /// Appends a new byte to the beginning of the given byte array and returns a new array with a bigger length.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="arr">Source array to append to.</param>
        /// <param name="newItem">The value to append to source.</param>
        /// <returns>The extended array of bytes.</returns>
        public static byte[] AppendToBeginning(this byte[] arr, byte newItem)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr), "Byte array can not be null!");


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
        /// <returns>The extended array of bytes.</returns>
        public static byte[] AppendToEnd(this byte[] arr, byte newItem)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr), "Byte array can not be null!");


            byte[] result = new byte[arr.Length + 1];
            result[result.Length - 1] = newItem;
            Buffer.BlockCopy(arr, 0, result, 0, arr.Length);
            return result;
        }


        /// <summary>
        /// Creates a copy (clone) of the given byte array, will return null if the source was null instead of throwing an exception.
        /// </summary>
        /// <param name="ba">Byte array to clone</param>
        /// <returns>Copy (clone) of the given byte array</returns>
        public static byte[] CloneByteArray(this byte[] ba)
        {
            if (ba == null)
            {
                return null;
            }
            else
            {
                byte[] result = new byte[ba.Length];
                Buffer.BlockCopy(ba, 0, result, 0, ba.Length);
                return result;
            }
        }


        /// <summary>
        /// Concatinates two given byte arrays and returns a new byte array containing all the elements. 
        /// </summary>
        /// <remarks>
        /// This is a lot faster than Linq (~30 times)
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="firstArray">First set of bytes in the final array.</param>
        /// <param name="secondArray">Second set of bytes in the final array.</param>
        /// <returns>The concatinated array of bytes.</returns>
        public static byte[] ConcatFast(this byte[] firstArray, byte[] secondArray)
        {
            if (firstArray == null)
                throw new ArgumentNullException(nameof(firstArray), "First array can not be null!");
            if (secondArray == null)
                throw new ArgumentNullException(nameof(secondArray), "Second array can not be null!");


            byte[] result = new byte[firstArray.Length + secondArray.Length];
            Buffer.BlockCopy(firstArray, 0, result, 0, firstArray.Length);
            Buffer.BlockCopy(secondArray, 0, result, firstArray.Length, secondArray.Length);
            return result;
        }


        /// <summary>
        /// Determines whether two byte arrays are equal.
        /// </summary>
        /// <remarks>
        /// This is A LOT faster than Linq.
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="first">First byte array for comparing.</param>
        /// <param name="second">Second byte array for comparing.</param>
        /// <returns>True if two sequences were equal, false if otherwise.</returns>
        public static bool IsEqualTo(this byte[] first, byte[] second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first), "First byte array can not be null!");
            if (second == null)
                throw new ArgumentNullException(nameof(second), "Second byte array can not be null!");


            // TODO: use Span<byte>.SequenceEqual() it is even faster than this method!
            if (first.Length != second.Length)
            {
                return false;
            }
            if (first.Length == 0) // lengths are equal if we are here
            {
                return true;
            }
            unsafe
            {
                fixed (byte* f = &first[0], s = &second[0])
                {
                    int len = first.Length;
                    byte* cfPt = f; // cfPt is changing first pointer
                    byte* csPt = s;
                    while (len >= 8)
                    {
                        if (*(ulong*)csPt != *(ulong*)cfPt)
                        {
                            return false;
                        }
                        cfPt += 8;
                        csPt += 8;
                        len -= 8;
                    }

                    // Here len can be anything from 0 to 7 and we take 4 of it if possible if>=4
                    if (len >= 4)
                    {
                        if (*(uint*)cfPt != *(uint*)csPt)
                        {
                            return false;
                        }
                        cfPt += 4;
                        csPt += 4;
                        len -= 4;
                    }

                    // Here len can be anything from 0 to 3
                    if (len >= 2)
                    {
                        if (*(ushort*)cfPt != *(ushort*)csPt)
                        {
                            return false;
                        }
                        cfPt += 2;
                        csPt += 2;
                        len -= 2;
                    }

                    // Here len can only be 0 or 1
                    if (len == 1)
                    {
                        if (*csPt != *cfPt)
                        {
                            return false;
                        }
                        // no need to change pointer and len anymore, this was the last item.
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Creates a new array from the given array by taking a specified number of items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index, int count)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), "Input can not be null!");
            if (index < 0 || count < 0)
                throw new IndexOutOfRangeException("Index or count can not be negative.");
            if (sourceArray.Length != 0 && index > sourceArray.Length - 1 || sourceArray.Length == 0 && index != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");
            if (count > sourceArray.Length - index)
                throw new IndexOutOfRangeException("Array is not long enough.");


            byte[] result = new byte[count];
            Buffer.BlockCopy(sourceArray, index, result, 0, count);
            return result;
        }


        /// <summary>
        /// Creates a new array from the given array by taking items starting from a given index.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="index">Starting index in <paramref name="sourceArray"/>.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArray(this byte[] sourceArray, int index)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), "Input can not be null!");
            if (sourceArray.Length != 0 && index > sourceArray.Length - 1 || sourceArray.Length == 0 && index != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");

            return SubArray(sourceArray, index, sourceArray.Length - index);
        }


        /// <summary>
        /// Creates a new array from the given array by taking the specified number of items from the end of the array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <param name="sourceArray">The array containing bytes to take.</param>
        /// <param name="count">Number of elements to take.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] SubArrayFromEnd(this byte[] sourceArray, int count)
        {
            if (sourceArray == null)
                throw new ArgumentNullException(nameof(sourceArray), $"Input can not be null!");
            if (count < 0)
                throw new IndexOutOfRangeException("Count can not be negative.");
            if (count > sourceArray.Length)
                throw new IndexOutOfRangeException("Array is not long enough.");

            return (count == 0) ? new byte[0] : sourceArray.SubArray(sourceArray.Length - count, count);
        }


        /// <summary>
        /// Converts the given byte array to base-16 (Hexadecimal) encoded string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <returns>Base-16 (Hexadecimal) encoded string.</returns>
        public static string ToBase16(this byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");


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
        /// Converts the given arbitrary length bytes to a its equivalant <see cref="BigInteger"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">The array of bytes to convert.</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <param name="treatAsPositive">If true will treat the given bytes as always a positive integer.</param>
        /// <returns>A BigInteger.</returns>
        public static BigInteger ToBigInt(this byte[] ba, bool isBigEndian, bool treatAsPositive)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");


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


        /// <summary>
        /// Converts the given four bytes to a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must be 4 bytes long).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 32-bit signed integer.</returns>
        public static int ToInt32(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            if (ba.Length != sizeof(int))
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 4 bytes.");


            unchecked
            {
                return isBigEndian ?
                    ba[3] | (ba[2] << 8) | (ba[1] << 16) | (ba[0] << 24) :
                    ba[0] | (ba[1] << 8) | (ba[2] << 16) | (ba[3] << 24);
            }
        }

        /// <summary>
        /// Converts the given two bytes to a 16-bit unsigned integer.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must be 2 bytes long).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 16-bit unsigned integer.</returns>
        public static ushort ToUInt16(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            if (ba.Length != sizeof(ushort))
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 2 bytes.");


            unchecked
            {
                return isBigEndian ?
                    (ushort)(ba[1] | (ba[0] << 8)) :
                    (ushort)(ba[0] | (ba[1] << 8));
            }
        }

        /// <summary>
        /// Converts the given two bytes to a 32-bit unsigned integer.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must be 4 bytes long).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 32-bit unsigned integer.</returns>
        public static uint ToUInt32(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            if (ba.Length != sizeof(uint))
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 4 bytes.");


            unchecked
            {
                return isBigEndian ?
                    (uint)(ba[3] | (ba[2] << 8) | (ba[1] << 16) | (ba[0] << 24)) :
                    (uint)(ba[0] | (ba[1] << 8) | (ba[2] << 16) | (ba[3] << 24));
            }
        }

        /// <summary>
        /// Converts the given two bytes to a 64-bit unsigned integer.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ba">The array of bytes to convert (must be 8 bytes long).</param>
        /// <param name="isBigEndian">Endianness of given bytes.</param>
        /// <returns>A 64-bit unsigned integer.</returns>
        public static ulong ToUInt64(this byte[] ba, bool isBigEndian)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            if (ba.Length != sizeof(ulong))
                throw new ArgumentOutOfRangeException(nameof(ba), ba.ToBase16(), "Byte array must be 8 bytes.");


            unchecked
            {
                return isBigEndian ?
                    ba[7] | ((ulong)ba[6] << 8) | ((ulong)ba[5] << 16) | ((ulong)ba[4] << 24) |
                            ((ulong)ba[3] << 32) | ((ulong)ba[2] << 40) | ((ulong)ba[1] << 48) | ((ulong)ba[0] << 56) :
                    ba[0] | ((ulong)ba[1] << 8) | ((ulong)ba[2] << 16) | ((ulong)ba[3] << 24) |
                            ((ulong)ba[4] << 32) | ((ulong)ba[5] << 40) | ((ulong)ba[6] << 48) | ((ulong)ba[7] << 56);
            }
        }


        /// <summary>
        /// Removes zeros from the end of the given byte array.
        /// <para/>NOTE: If there is no zeros to trim, the same byte array will be return (careful about changing the reference)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to trim.</param>
        /// <returns>Trimmed bytes.</returns>
        public static byte[] TrimEnd(this byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null!");


            int index = ba.Length - 1;
            int count = 0;
            while (index >= 0 && ba[index] == 0)
            {
                index--;
                count++;
            }
            return (count == 0) ? ba : (count == ba.Length) ? new byte[0] : ba.SubArray(0, ba.Length - count);
        }

        /// <summary>
        /// Removes zeros from the beginning of the given byte array.
        /// <para/>NOTE: If there is no zeros to trim, the same byte array will be return (careful about changing the reference)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ba">Byte array to trim.</param>
        /// <returns>Trimmed bytes.</returns>
        public static byte[] TrimStart(this byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null!");


            int index = 0;// index acts both as "index" and "count"
            while (index != ba.Length && ba[index] == 0)
            {
                index++;
            }
            return (index == 0) ? ba : (index == ba.Length) ? new byte[0] : ba.SubArray(index);
        }

    }





    public static class IntExtension
    {
        /// <summary>
        /// Converts the given 32-bit signed integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 32-bit signed integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this int i, bool bigEndian)
        {
            unchecked
            {
                if (bigEndian)
                {
                    return new byte[]
                    {
                        (byte)(i >> 24),
                        (byte)(i >> 16),
                        (byte)(i >> 8),
                        (byte)i
                    };
                }
                else
                {
                    return new byte[]
                    {
                        (byte)i,
                        (byte)(i >> 8),
                        (byte)(i >> 16),
                        (byte)(i >> 24)
                    };
                }
            }
        }

    }


    public static class LongExtension
    {
        /// <summary>
        /// Converts the given 64-bit signed integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 64-bit signed integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this long i, bool bigEndian)
        {
            unchecked
            {
                if (bigEndian)
                {
                    return new byte[]
                    {
                        (byte)(i >> 56),
                        (byte)(i >> 48),
                        (byte)(i >> 40),
                        (byte)(i >> 32),
                        (byte)(i >> 24),
                        (byte)(i >> 16),
                        (byte)(i >> 8),
                        (byte)i
                    };
                }
                else
                {
                    return new byte[]
                    {
                        (byte)i,
                        (byte)(i >> 8),
                        (byte)(i >> 16),
                        (byte)(i >> 24),
                        (byte)(i >> 32),
                        (byte)(i >> 40),
                        (byte)(i >> 48),
                        (byte)(i >> 56)
                    };
                }
            }
        }

    }


    public static class UIntExtension
    {
        /// <summary>
        /// Converts the given 16-bit unsigned integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 16-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this ushort i, bool bigEndian)
        {
            unchecked
            {
                if (bigEndian)
                {
                    return new byte[]
                    {
                        (byte)(i >> 8),
                        (byte)i
                    };
                }
                else
                {
                    return new byte[]
                    {
                        (byte)i,
                        (byte)(i >> 8)
                    };
                }
            }
        }

        /// <summary>
        /// Converts the given 32-bit unsigned integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 32-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this uint i, bool bigEndian)
        {
            unchecked
            {
                if (bigEndian)
                {
                    return new byte[]
                    {
                        (byte)(i >> 24),
                        (byte)(i >> 16),
                        (byte)(i >> 8),
                        (byte)i
                    };
                }
                else
                {
                    return new byte[]
                    {
                        (byte)i,
                        (byte)(i >> 8),
                        (byte)(i >> 16),
                        (byte)(i >> 24)
                    };
                }
            }
        }

        /// <summary>
        /// Converts the given 64-bit unsigned integer to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="i">The 64-bit unsigned integer to convert.</param>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this ulong i, bool bigEndian)
        {
            unchecked
            {
                if (bigEndian)
                {
                    return new byte[]
                    {
                        (byte)(i >> 56),
                        (byte)(i >> 48),
                        (byte)(i >> 40),
                        (byte)(i >> 32),
                        (byte)(i >> 24),
                        (byte)(i >> 16),
                        (byte)(i >> 8),
                        (byte)i
                    };
                }
                else
                {
                    return new byte[]
                    {
                        (byte)i,
                        (byte)(i >> 8),
                        (byte)(i >> 16),
                        (byte)(i >> 24),
                        (byte)(i >> 32),
                        (byte)(i >> 40),
                        (byte)(i >> 48),
                        (byte)(i >> 56)
                    };
                }
            }
        }

    }




    public static class UnixTimeStamp
    {
        /// <summary>
        /// Converts a given <see cref="DateTime"/> to Epoch time.
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/> to convert</param>
        /// <returns>Epoch time</returns>
        public static long TimeToEpoch(DateTime dt)
        {
            return ((DateTimeOffset)DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Converts a given Epoch time into its <see cref="DateTime"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="epoch">Epoch time</param>
        /// <returns>Converted <see cref="DateTime"/></returns>
        public static DateTime EpochToTime(long epoch)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
        }

        /// <summary>
        /// Returns current system Epoch time.
        /// </summary>
        /// <returns>Current Epoch time</returns>
        public static long GetEpochNow()
        {
            return TimeToEpoch(DateTime.Now);
        }

        /// <summary>
        /// Returns current UTC Epoch time.
        /// </summary>
        /// <returns>Current UTC Epoch time.</returns>
        public static long GetEpochUtcNow()
        {
            return TimeToEpoch(DateTime.UtcNow);
        }

    }

}
