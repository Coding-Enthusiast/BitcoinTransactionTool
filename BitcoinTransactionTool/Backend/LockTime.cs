// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace BitcoinTransactionTool.Backend
{
    /// <summary>
    /// The block number or timestamp at which the transaction is unlocked.
    /// </summary>
    /// <remarks>
    /// LockTime is interpreted based on sequences and BIP68
    /// </remarks>
    public readonly struct LockTime : IComparable, IComparable<LockTime>, IEquatable<LockTime>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> using a 32-bit signed integer.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use (must be >= 0)</param>
        public LockTime(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "Locktime value can not be negative!");

            Value = (uint)val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> using a 32-bit unsigned integer.
        /// </summary>
        /// <param name="val">Value to use</param>
        public LockTime(uint val)
        {
            Value = val;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> using a DateTime instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="val">Value to use</param>
        public LockTime(DateTime val)
        {
            long num = UnixTimeStamp.TimeToEpoch(val);
            if (num < Threshold || num >= uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(val), "DateTime value is not a valid Locktime.");
            }
            Value = (uint)num;
        }


        /// <summary>
        /// Integer value of this instance.
        /// </summary>
        internal readonly uint Value;

        /// <summary>
        /// Represents the largest possible value of System.UInt32. This field is constant.
        /// </summary>
        public const uint Threshold = 500_000_000U;



        /// <summary>
        /// The minimum LockTime value (0).
        /// </summary>
        public static LockTime Minimum => new LockTime(0U);

        /// <summary>
        /// The maximum LockTime value (0xFFFFFFFF).
        /// </summary>
        public static LockTime Maximum => new LockTime(uint.MaxValue);



        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> by reading it from a byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="data">Byte array containing a <see cref="LockTime"/>.</param>
        /// <returns>A new instance of <see cref="LockTime"/></returns>
        public static LockTime ReadFromBytes(byte[] data)
        {
            int i = 0;
            return ReadFromBytes(data, ref i);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LockTime"/> by reading it from a byte array 
        /// starting from the given offset and changing it based on the length of the data that was read.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="data">Byte array containing a <see cref="LockTime"/>.</param>
        /// <param name="offset">Offset in <paramref name="data"/> to start reading from.</param>
        /// <returns>A new instance of <see cref="LockTime"/></returns>
        public static LockTime ReadFromBytes(byte[] data, ref int offset)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Data can not be null or empty!");
            if (offset < 0)
                throw new IndexOutOfRangeException("Offset can not be negative.");
            if (offset + 4 > data.Length)
                throw new IndexOutOfRangeException("Offset is bigger than data length.");


            if (!TryReadFromBytes(data, ref offset, out LockTime result, out string error))
            {
                throw new FormatException(error);
            }
            return result;
        }


        /// <summary>
        /// Reads the <see cref="LockTime"/> value from the given byte array starting from the specified offset, 
        /// changing that offset based on the length of data that was read. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing a <see cref="LockTime"/>.</param>
        /// <param name="offset">Offset in <paramref name="data"/> to start reading from.</param>
        /// <param name="result">The result</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public static bool TryReadFromBytes(byte[] data, ref int offset, out LockTime result, out string error)
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
            if (data.Length - offset < 4)
            {
                error = "Data length is not valid.";
                result = 0;
                return false;
            }

            result = data.SubArray(offset, 4).ToUInt32(false);
            offset += 4;
            error = null;
            return true;
        }


        /// <summary>
        /// Converts this instance to an array of bytes with a desired endianness.
        /// </summary>
        /// <param name="bigEndian">Endianness of the returned byte array.</param>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArray(bool bigEndian = false)
        {
            return Value.ToByteArray(bigEndian);
        }


        public static implicit operator LockTime(uint val)
        {
            return new LockTime(val);
        }
        public static implicit operator LockTime(ushort val)
        {
            return new LockTime(val);
        }
        public static implicit operator LockTime(byte val)
        {
            return new LockTime(val);
        }
        public static explicit operator LockTime(int val)
        {
            if (val < 0)
                throw new InvalidCastException("Locktime can not be negative");

            return new LockTime((uint)val);
        }

        public static implicit operator uint(LockTime val)
        {
            return val.Value;
        }
        public static explicit operator ushort(LockTime val)
        {
            return checked((ushort)val.Value);
        }
        public static explicit operator byte(LockTime val)
        {
            return checked((byte)val.Value);
        }
        public static explicit operator int(LockTime val)
        {
            return checked((int)val.Value);
        }


        public static explicit operator LockTime(DateTime val)
        {
            long num = UnixTimeStamp.TimeToEpoch(val);
            if (num < Threshold || num >= uint.MaxValue)
            {
                throw new InvalidCastException("Locktime value is not valid to be a DateTime!");
            }

            return new LockTime((uint)num);
        }
        public static explicit operator DateTime(LockTime val)
        {
            if (val.Value < Threshold || val.Value == uint.MaxValue)
                throw new InvalidCastException("Locktime is not a valid DateTime instant.");

            return UnixTimeStamp.EpochToTime(val.Value);
        }


        public static bool operator >(LockTime left, LockTime right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator >=(LockTime left, LockTime right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator <(LockTime left, LockTime right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator <=(LockTime left, LockTime right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator ==(LockTime left, LockTime right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(LockTime left, LockTime right)
        {
            return left.CompareTo(right) != 0;
        }


        public static LockTime operator ++(LockTime lt)
        {
            return lt.Value == uint.MaxValue ? LockTime.Maximum : new LockTime(lt.Value + 1);
        }

        /// <summary>
        /// Returns a new instance of <see cref="LockTime"/> with its value increased by one up to its maximum value
        /// without changing this instance. If you want to change this instance's value use the ++ operator.
        /// </summary>
        public LockTime Increment()
        {
            return Value == uint.MaxValue ? LockTime.Maximum : new LockTime(Value + 1);
        }



        #region Interfaces and overrides

        /// <summary>
        /// Compares the value of a given <see cref="LockTime"/> with the value of this instance and 
        /// Returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <param name="other">Other <see cref="LockTime"/> to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger.</returns>
        public int CompareTo(LockTime other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="LockTime"/> and then compares its value with the value of this instance.
        /// And returns -1 if smaller, 0 if equal and 1 if bigger.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>-1 if smaller, 0 if equal and 1 if bigger</returns>
        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            if (!(obj is LockTime))
                throw new ArgumentException($"Object must be of type {nameof(LockTime)}");

            return CompareTo((LockTime)obj);
        }

        /// <summary>
        /// Checks if the value of the given <see cref="LockTime"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="LockTime"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(LockTime other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Checks if the given object is of type <see cref="LockTime"/> and if its value is equal to the value of this instance.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// true if value is an instance of <see cref="LockTime"/> 
        /// and equals the value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is LockTime))
                throw new ArgumentException($"Object must be of type {nameof(LockTime)}");

            return Equals((LockTime)obj);
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
            return (Value < Threshold || Value == uint.MaxValue) ? $"{Value}" : $"{UnixTimeStamp.EpochToTime(Value)}";
        }

        #endregion

    }
}
