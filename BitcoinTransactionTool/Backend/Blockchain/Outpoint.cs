// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Linq;

namespace BitcoinTransactionTool.Backend.Blockchain
{
    public class Outpoint : IDeserializable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Outpoint"/> using parameters of the <see cref="ICoin"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="coin">Coin to use</param>
        public Outpoint()
        {
            reverseTxId = true;
            HashSize = 32;
            Size = sizeof(uint) + HashSize;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Outpoint"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash of the transaction that is being spent as returned by the hash function (no reversal).</param>
        /// <param name="index">Index of transaction.</param>
        public Outpoint(byte[] hash, uint index)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Transaction hash can not be null.");
            if (hash.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Transaction hash must be 32 bytes.");

            TxHash = hash.CloneByteArray();
            Index = index;

            reverseTxId = true;
            HashSize = 32;
            Size = sizeof(uint) + HashSize;
        }



        public byte[] TxHash { get; set; }
        public uint Index { get; set; }

        internal readonly int HashSize;
        internal readonly int Size;
        private readonly bool reverseTxId;



        /// <summary>
        /// Returns transaction ID of this <see cref="Outpoint"/> in base-16.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>Base-16 encoded transaction ID.</returns>
        public string GetTxId()
        {
            if (TxHash == null)
                throw new ArgumentNullException(nameof(TxHash), "Transaction hash can not be null");

            return reverseTxId ? TxHash.Reverse().ToArray().ToBase16() : TxHash.ToBase16();
        }


        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>An array of bytes.</returns>
        public byte[] Serialize()
        {
            if (TxHash == null)
                throw new ArgumentNullException(nameof(TxHash), "Transaction hash can not be null.");

            return ByteArray.ConcatArrays(
                TxHash,
                Index.ToByteArray(false)
                );
        }


        /// <summary>
        /// Deserializes the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing an <see cref="Outpoint"/>.</param>
        /// <param name="offset">The offset inside the <paramref name="data"/> to start from.</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure).</param>
        /// <returns>True if deserialization was successful, false if otherwise.</returns>
        public bool TryDeserialize(byte[] data, ref int offset, out string error)
        {
            if (offset < 0)
            {
                error = "Offset can not be negative.";
                return false;
            }
            if (data == null || data.Length - offset < Size)
            {
                error = "Data length is not valid.";
                return false;
            }


            TxHash = data.SubArray(offset, HashSize);
            offset += HashSize;

            Index = data.SubArray(offset, sizeof(uint)).ToUInt32(false);
            offset += sizeof(uint);

            error = null;
            return true;
        }

    }
}
