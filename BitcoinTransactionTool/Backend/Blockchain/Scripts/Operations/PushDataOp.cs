// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operations that push any data onto the stack. Covers number OPs from <see cref="OP._0"/> to <see cref="OP._16"/>, 
    /// push OPs from byte=0x01 to 0x4b and <see cref="OP.PushData1"/>, <see cref="OP.PushData2"/> and <see cref="OP.PushData4"/>.
    /// </summary>
    public class PushDataOp : IOperation
    {
        /// <summary>
        /// Initializes an empty new instance of <see cref="PushDataOp"/>. Used for reading data from a stream.
        /// </summary>
        public PushDataOp()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <param name="ba">Byte array to use</param>
        public PushDataOp(byte[] ba)
        {
            if (ba == null)
                throw new ArgumentNullException(nameof(ba), "Byte array can not be null.");
            if (OpHelper.HasOpNum(ba)) // TODO: add isStrict field for this check.
                throw new ArgumentException("Short form of data exists which should be used instead.");

            data = ba.CloneByteArray();
            StackInt size = new StackInt(ba.Length);
            OpValue = size.GetOpCode();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="script">
        /// Script to use (will be converted to byte array using the <see cref="IScript.ToByteArray"/> function)
        /// </param>
        public PushDataOp(IScript script)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script), "Script can not be null.");

            data = script.ToByteArray();
            StackInt size = new StackInt(data.Length);
            OpValue = size.GetOpCode();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using one of the number OP codes.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="numOp">A number OP code (OP_0, OP_1, ..., OP_16, OP_Negative1</param>
        public PushDataOp(OP numOp)
        {
            if (!OpHelper.IsNumberOp(numOp))
                throw new ArgumentException("OP is not a numbered one.");

            data = null;
            OpValue = numOp;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PushDataOp"/> using the given integer.
        /// </summary>
        /// <param name="num">Integer value to use</param>
        public PushDataOp(int num)
        {
            if (num >= -1 && num <= 16) // We have OP for these
            {
                OpValue = OpHelper.IntToOp(num);
                data = null;
            }
            else // There is no OP, we have to use regular push
            {
                // TODO: this is wrong!!!
                data = OpHelper.IntToByteArray(num);
                StackInt size = new StackInt(data.Length);
                OpValue = size.GetOpCode();
            }
        }



        /// <summary>
        /// A single byte inticating type of the opeartion
        /// </summary>
        public OP OpValue { get; private set; }
        internal byte[] data;


        // TODO: this function is a bad workaround for tests. just let the test access the internal field!
        public byte[] GetData()
        {
            return data;
        }


        /// <summary>
        /// Pushes the specified data of this instance at the top of the stack.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public virtual bool Run(IOpData opData, out string error)
        {
            if (OpValue == OP._0)
            {
                opData.Push(new byte[0]);
            }
            else if (OpValue == OP.Negative1)
            {
                opData.Push(new byte[1] { 0b1000_0001 });
            }
            else if (OpValue >= OP._1 && OpValue <= OP._16)
            {
                // OP_1=0x51, OP_2=0x52, ...
                opData.Push(new byte[] { (byte)(OpValue - 0x50) });
            }
            else
            {
                opData.Push(data);
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Returns the appropriate byte array representation of this operation based on its type and data. 
        /// Used by Serialize() methods (not to be confused with what <see cref="Run(IOpData, out string)"/> does).
        /// </summary>
        /// <param name="isWitness">[Default value = false] Indicates whether this operation is inside a witness script</param>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArray(bool isWitness = false)
        {
            // TODO: double check if this is the same for witness and non-witness. it seems to be for OP_0 (multi-sig signatures)
            if (OpValue == OP._0 || OpValue == OP.Negative1)
            {
                return new byte[1] { (byte)OpValue };
            }
            else if (OpValue >= OP._1 && OpValue <= OP._16)
            {
                return new byte[1] { (byte)OpValue };
            }
            else if (isWitness)
            {
                CompactInt size = new CompactInt(data.Length);
                return size.ToByteArray().ConcatFast(data);
            }
            else
            {
                StackInt size = new StackInt(data.Length);
                return size.ToByteArray().ConcatFast(data);
            }
        }


        /// <summary>
        /// If possible, will convert the data of this instance to a 64-bit signed integer. The return value indicates success. 
        /// </summary>
        /// <param name="isStrict">Indicates whether to use strict rules</param>
        /// <param name="result">The equivalant 64-bit signed integer or zero in case of failure</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if deserialization was successful, false if otherwise</returns>
        public bool TryGetNumber(bool isStrict, out long result, out string error)
        {
            if (data == null)
            {
                if (OpValue == OP._0)
                {
                    result = 0;
                }
                else if (OpValue == OP.Negative1)
                {
                    result = -1;
                }
                else if (OpValue >= OP._1 && OpValue <= OP._16)
                {
                    result = (byte)OpValue - 0x50;
                }
                else
                {
                    result = 0;
                    error = "No data is available.";
                    return false;
                }
            }
            else
            {
                if (!OpHelper.TryConvertByteArrayToInt(data, out result, isStrict))
                {
                    error = "Invalid number format.";
                    return false;
                }
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Reads the push data from the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array to use</param>
        /// <param name="offset">The offset inside the <paramref name="data"/> to start from.</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <param name="isWitness">Indicates whether this operation is inside a witness script</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryRead(byte[] data, ref int offset, out string error, bool isWitness = false)
        {
            if (data == null || data.Length - offset < 1)
            {
                error = "Data length is not valid.";
                return false;
            }

            // TODO: set a bool for isStrict so that we can reject non-strict encoded lengths

            OpValue = (OP)data[offset];
            int dataSize = 0;
            // TODO: check if this assumption is correct: witness can have OP_number values and they are the same as in other scripts
            if (data[offset] == (byte)OP._0 || data[offset] == (byte)OP.Negative1 ||
                data[offset] >= (byte)OP._1 && data[offset] <= (byte)OP._16)
            {
                this.data = null;
                offset++;

                error = null;
                return true; // This has to return here since we set the data at the bottom after if block ends
            }
            else if (isWitness)
            {
                if (!CompactInt.TryReadFromBytes(data, ref offset, out CompactInt size, out error))
                {
                    return false;
                }

                if (size.Value > int.MaxValue)
                {
                    error = "Push data size is too big.";
                    return false;
                }
                dataSize = (int)size;
            }
            else
            {
                if (!StackInt.TryReadFromBytes(data, ref offset, out StackInt size, out error))
                {
                    return false;
                }

                if (size.Value > int.MaxValue)
                {
                    error = "Push data size is too big.";
                    return false;
                }
                dataSize = (int)size;
            }


            if (dataSize > data.Length - offset)
            {
                error = "Data length is not valid.";
                return false;
            }

            this.data = data.SubArray(offset, dataSize);
            offset += dataSize;

            error = null;
            return true;
        }



        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object, flase if otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is PushDataOp)
            {
                if (((PushDataOp)obj).OpValue == OpValue)
                {
                    if (((PushDataOp)obj).data == null)
                    {
                        return data == null;
                    }
                    else
                    {
                        return ((PushDataOp)obj).data.IsEqualTo(data);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode()
        {
            if (data == null)
            {
                return OpValue.GetHashCode();
            }
            else
            {
                int hash = 17;
                foreach (var b in data)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }

    }
}
