// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using System;

namespace BitcoinTransactionTool.Backend
{
    /// <summary>
    /// Represents values used in bitcoin (and its copies) script stacks indicating length of the data to push to stack.
    /// </summary>
    public readonly struct StackInt : IComparable, IComparable<StackInt>, IEquatable<StackInt>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StackInt"/> using a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public StackInt(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "StackInt value can not be negative!");

            Value = (uint)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StackInt"/> using a 32-bit unsigned integer.
        /// </summary>
        /// <param name="val">Value to use</param>
        public StackInt(uint val)
        {
            Value = val;
        }



        /// <summary>
        /// Integer value of this instance.
        /// </summary>
        internal readonly uint Value;



        /// <summary>
        /// Returns the OP code used as the first byte when converting to a byte array
        /// </summary>
        /// <returns><see cref="OP"/> code</returns>
        public OP GetOpCode()
        {
            if (Value > ushort.MaxValue)
            {
                return OP.PushData4;
            }
            else if (Value > byte.MaxValue)
            {
                return OP.PushData2;
            }
            else if (Value > (byte)OP.PushData1)
            {
                return OP.PushData1;
            }
            else
            {
                return (OP)Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StackInt"/> by reading it from a byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="data">Byte array containing a <see cref="StackInt"/>.</param>
        /// <returns>A new instance of <see cref="StackInt"/></returns>
        public static StackInt ReadFromBytes(byte[] data)
        {
            int i = 0;
            return ReadFromBytes(data, ref i);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StackInt"/> by reading it from a byte array 
        /// starting from the given offset and changing it based on the length of the data that was read.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="data">Byte array containing a <see cref="StackInt"/>.</param>
        /// <param name="offset">Offset in <paramref name="data"/> to start reading from.</param>
        /// <returns>A new instance of <see cref="StackInt"/></returns>
        public static StackInt ReadFromBytes(byte[] data, ref int offset)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Data can not be null or empty!");
            if (offset < 0)
                throw new IndexOutOfRangeException("Offset can not be negative.");
            if (offset >= data.Length)
                throw new IndexOutOfRangeException("Offset is bigger than data length.");

            if (!TryReadFromBytes(data, ref offset, out StackInt result, out string error))
            {
                throw new FormatException(error);
            }
            return result;
        }


        /// <summary>
        /// Reads the <see cref="StackInt"/> value from the given byte array starting from the specified offset,
        /// changing that offset based on the length of data that was read. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing a <see cref="StackInt"/>.</param>
        /// <param name="offset">Offset in <paramref name="data"/> to start reading from.</param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryReadFromBytes(byte[] data, ref int offset, out StackInt result, out string error)
        {
            if (data == null || data.Length == 0)
            {
                error = "Data can not be null or empty.";
                result = 0;
                return false;
            }
            if (offset < 0)
            {
                error = "Offset can not be negative.";
                result = 0;
                return false;
            }
            if (data.Length - offset < 1)
            {
                error = "Data length is not valid.";
                result = 0;
                return false;
            }


            if (data[offset] < (byte)OP.PushData1)
            {
                result = new StackInt((uint)data[offset]);
                offset++;
            }
            else if (data[offset] == (byte)OP.PushData1)
            {
                if (data.Length - offset < 2)
                {
                    error = $"With OP_{OP.PushData1} data length must be at least 2 bytes.";
                    result = 0;
                    return false;
                }
                byte val = data[offset + 1];
                if (val < (byte)OP.PushData1)
                {
                    error = $"With OP_{OP.PushData1} the push size must be 1 byte and bigger than {(byte)OP.PushData1 - 1}.";
                    result = 0;
                    return false;
                }
                result = new StackInt((uint)val);
                offset += 2;
            }
            else if (data[offset] == (byte)OP.PushData2)
            {
                if (data.Length - offset < 3)
                {
                    error = $"With OP_{OP.PushData2} data length must be at least 3 bytes.";
                    result = 0;
                    return false;
                }
                ushort val = data.SubArray(offset + 1, 2).ToUInt16(false);
                if (val <= byte.MaxValue)
                {
                    error = $"With OP_{OP.PushData2} the push size must be bigger than 1 byte.";
                    result = 0;
                    return false;
                }
                result = new StackInt((uint)val);
                offset += 3;
            }
            else if (data[offset] == (byte)OP.PushData4)
            {
                if (data.Length - offset < 5)
                {
                    error = $"With OP_{OP.PushData4} data length must be at least 5 bytes.";
                    result = 0;
                    return false;
                }
                uint val = data.SubArray(offset + 1, 4).ToUInt32(false);
                if (val <= ushort.MaxValue)
                {
                    error = $"With OP_{OP.PushData4} the push size must be bigger than 2 byte.";
                    result = 0;
                    return false;
                }
                result = new StackInt(val);
                offset += 5;
            }
            else
            {
                error = "Unknown OP_Push value.";
                result = 0;
                return false;
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Converts this value to its byte array representation in little-endian order.
        /// </summary>
        /// <returns>An array of bytes in little-endian order</returns>
        public byte[] ToByteArray()
        {
            if (Value < (byte)OP.PushData1)
            {
                return new byte[] { (byte)Value };
            }
            else if (Value <= byte.MaxValue)
            {
                return new byte[] { (byte)OP.PushData1, (byte)Value };
            }
            else if (Value <= ushort.MaxValue)
            {
                return new byte[] { (byte)OP.PushData2 }.ConcatFast(((ushort)Value).ToByteArray(false));
            }
            else // Value <= uint.MaxValue
            {
                return new byte[] { (byte)OP.PushData4 }.ConcatFast(Value.ToByteArray(false));
            }
        }


        public static implicit operator StackInt(uint val)
        {
            return new StackInt(val);
        }
        public static implicit operator StackInt(ushort val)
        {
            return new StackInt(val);
        }
        public static implicit operator StackInt(byte val)
        {
            return new StackInt(val);
        }
        public static explicit operator StackInt(int val)
        {
            if (val < 0)
                throw new InvalidCastException("StackInt can not be negative");

            return new StackInt((uint)val);
        }


        public static implicit operator uint(StackInt val)
        {
            return val.Value;
        }
        public static explicit operator ushort(StackInt val)
        {
            return checked((ushort)val.Value);
        }
        public static explicit operator byte(StackInt val)
        {
            return checked((byte)val.Value);
        }
        public static explicit operator int(StackInt val)
        {
            return checked((int)val.Value);
        }


        public static bool operator >(StackInt left, StackInt right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(StackInt left, StackInt right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator <(StackInt left, StackInt right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(StackInt left, StackInt right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator ==(StackInt left, StackInt right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(StackInt left, StackInt right)
        {
            return left.CompareTo(right) != 0;
        }


        #region Interfaces and overrides

        /// <summary>
        /// Compares the value of a given <see cref="StackInt"/> with the value of this instance and 
        /// And returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <param name="other">Other <see cref="StackInt"/> to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger.</returns>
        public int CompareTo(StackInt other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="StackInt"/> and then compares its value with the value of this instance.
        /// Returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger</returns>
        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            if (!(obj is StackInt))
                throw new ArgumentException($"Object must be of type {nameof(StackInt)}");

            return CompareTo((StackInt)obj);
        }

        /// <summary>
        /// Checks if the value of the given <see cref="StackInt"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="StackInt"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(StackInt other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="StackInt"/> and if its value is equal to the value of this instance.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// true if value is an instance of <see cref="StackInt"/> 
        /// and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is StackInt))
                throw new ArgumentException($"Object must be of type {nameof(StackInt)}");

            return Equals((StackInt)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Converts the value of the current instance to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the value of the current instance.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

    }
}
