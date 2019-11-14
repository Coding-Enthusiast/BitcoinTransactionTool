// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation that is used to include an arbitrary data in transactions, it will fail on running.
    /// </summary>
    public class ReturnOp : NotRunableOps
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ReturnOp"/> with an empty data (0x6a).
        /// </summary>
        public ReturnOp()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ReturnOp"/> using the given data.
        /// </summary>
        /// <param name="ba">Data to use (can be null)</param>
        /// <param name="usePushOp">
        /// [Default value = true]
        /// If true, the data will be included after <see cref="OP.RETURN"/> using <see cref="PushDataOp"/> scheme.
        /// </param>
        public ReturnOp(byte[] ba, bool usePushOp = true)
        {
            if (ba == null || ba.Length == 0)
            {
                Data = new byte[1] { (byte)OP.RETURN };
            }
            else if (usePushOp)
            {
                StackInt size = new StackInt(ba.Length);
                Data = ByteArray.ConcatArrays(
                    new byte[1] { (byte)OP.RETURN },
                    size.ToByteArray(),
                    ba);
            }
            else
            {
                Data = new byte[ba.Length + 1];
                Data[0] = (byte)OP.RETURN;
                Buffer.BlockCopy(ba, 0, Data, 1, ba.Length);
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ReturnOp"/> using the given <see cref="IScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="scr">Script to use</param>
        /// <param name="usePushOp">
        /// [Default value = true]
        /// If true, the data will be included after <see cref="OP.RETURN"/> using <see cref="PushDataOp"/> scheme.
        /// </param>
        public ReturnOp(IScript scr, bool usePushOp = true)
        {
            if (scr == null)
                throw new ArgumentNullException(nameof(scr), "Script can not be null.");

            byte[] temp = scr.ToByteArray();

            if (usePushOp)
            {
                StackInt size = new StackInt(temp.Length);
                Data = ByteArray.ConcatArrays(
                    new byte[1] { (byte)OP.RETURN },
                    size.ToByteArray(),
                    temp);
            }
            else
            {
                Data = new byte[temp.Length + 1];
                Data[0] = (byte)OP.RETURN;
                Buffer.BlockCopy(temp, 0, Data, 1, temp.Length);
            }
        }



        // TODO: change accessibility to internal and let test project access it only
        public byte[] Data { get; private set; }

        /// <summary>
        /// A single byte inticating type of the opeartion
        /// </summary>
        public override OP OpValue => OP.RETURN;



        /// <summary>
        /// Returns the byte array representation of this operation based on its type and data. 
        /// Used by Serialize() methods.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] ToByteArray()
        {
            if (Data == null || Data.Length == 0)
            {
                return new byte[1] { (byte)OP.RETURN };
            }
            else
            {
                return Data.CloneByteArray();
            }
        }


        /// <summary>
        /// Reads the <see cref="OP.RETURN"/> byte and the following specified data length from the specified offset. 
        /// The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array to use</param>
        /// <param name="offset">The offset inside the <paramref name="data"/> to start from.</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <param name="length">
        /// Length of the data to read. Must be at least 1 byte, the <see cref="OP.RETURN"/> itself. 
        /// (note that <see cref="OP.RETURN"/> doesn't have any internal mechanism to tell us how much data it holds, 
        /// the length is instead specified before <see cref="OP.RETURN"/> as the length of the whole script).
        /// </param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryRead(byte[] data, ref int offset, out string error, int length)
        {
            if (offset < 0)
            {
                error = "Offset can not be negative.";
                return false;
            }
            if (length < 1)
            {
                error = "Lengh can not be smaller than 1.";
                return false;
            }
            if (data == null || data.Length - offset < length)
            {
                error = "Data length is not valid.";
                return false;
            }
            if (data[offset] != (byte)OP.RETURN)
            {
                error = $"OP at offset={offset} is not equal to OP_Return.";
                return false;
            }

            Data = data.SubArray(offset, length);
            offset += length;

            error = null;
            return true;
        }
    }
}
