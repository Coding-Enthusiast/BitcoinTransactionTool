// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    internal static class OpHelper
    {
        /// <summary>
        /// Checks whether a given byte array is zero (...,0,0,0,0) or negative zero (...,0,0,0x80)
        /// <para/> This is the same as IsTrue()
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if given bytes represented zero or negative zero; False if otherwise.</returns>
        internal static bool IsNotZero(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    // Can be negative zero
                    if (i == data.Length - 1 && data[i] == 0x80)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }


        // This method is supposed to convert the bytes that are already in the stack (not in scripts) to a numeric value.
        // This method is called by the following methods and each require a specific size of data:
        // - ArithmeticOps that need values between(-2^31 +1) and(2^31 -1) (0xffffff7f) => 4 bytes
        // - Multisig m of n that needs values between 0 and 20 => 1 byte
        // - Locktimes that need values between 0 and(2^39-1) (0xffffffff7f) => 5 bytes
        // 
        // So we choose the biggest type we have(Int64) but the caller is responsible to check if returned value is in range or not.
        //
        // IsStrict is only there to ensure shortest form was chosen to encode a number: 1 = {1} instead of {1,0,0}
        public static bool TryConvertByteArrayToInt(byte[] data, out long result, bool isStrict)
        {
            result = 0;
            if (data == null || data.Length > sizeof(long))
            {
                return false;
            }

            if (data.Length == 0)
            {
                return true;
            }

            if (isStrict && (data[data.Length - 1] & 0b0111_1111) == 0)
            {
                if (data.Length <= 1 || (data[data.Length - 2] & 0b1000_0000) == 0)
                {
                    return false;
                }
            }

            for (int i = 0; i < data.Length; i++)
            {
                result |= (long)data[i] << (8 * i);
            }

            if ((data[data.Length - 1] & 0b1000_0000) != 0)
            {
                result &= ~(0b1000_0000 << (8 * (data.Length - 1)));
                result *= -1;
            }

            return true;
        }


        public static byte[] IntToByteArray(long val)
        {
            if (val == 0)
            {
                return new byte[0];
            }

            if (val >= 0)
            {
                byte[] data = val.ToByteArray(false).TrimEnd();
                if ((data[data.Length - 1] & 0b1000_0000) != 0)
                {
                    data = data.AppendToEnd(0);
                }
                return data;
            }
            else
            {
                byte[] data = (-val).ToByteArray(false).TrimEnd();
                if ((data[data.Length - 1] & 0b1000_0000) == 0)
                {
                    data[data.Length - 1] |= 0b1000_0000;
                }
                else
                {
                    data = data.AppendToEnd(0b1000_0000);
                }
                return data;
            }
        }


        internal static OP IntToOp(int val)
        {
            if (val == 0)
            {
                return OP._0;
            }
            else if (val == -1)
            {
                return OP.Negative1;
            }
            else if (val >= 1 && val <= 16)
            {
                // OP_1 = 0x51, OP_2 = 0x52,...
                return (OP)(val + 0x50);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(val), "There is no OP defined for values outside of ∈[-1,16].");
            }
        }


        public static bool HasOpNum(byte[] data)
        {
            if (data.Length == 0)
            {
                return true;
            }
            else if (data.Length == 1)
            {
                if (data[0] == 0b10000000 /*-1*/ || data[0] >= 0 && data[0] <= 16)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsNumberOp(OP val)
        {
            return !(val != OP._0 && val != OP.Negative1 && val < OP._1 || val > OP._16);
        }

    }
}
