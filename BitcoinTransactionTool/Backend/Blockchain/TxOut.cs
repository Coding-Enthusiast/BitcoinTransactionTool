// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using System;

namespace BitcoinTransactionTool.Backend.Blockchain
{
    public class TxOut : IDeserializable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TxOut"/> using given parameters of the <see cref="ICoin"/>.
        /// </summary>
        public TxOut()
        {
            PubScript = new PubkeyScript();
            maxAmount = 21_000_000_0000_0000;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TxOut"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="amount">Payment amount in coin's smallest unit (eg. Satoshi).</param>
        /// <param name="pkScript">Public key script</param>
        public TxOut(ulong amount, PubkeyScript pkScript)
        {
            if (amount > 21_000_000_0000_0000)
                throw new ArgumentOutOfRangeException(nameof(amount), "Can not spend more than coin's maximum supply.");
            if (pkScript == null)
                throw new ArgumentNullException(nameof(pkScript), $"Pubkey script can not be null.");

            Amount = amount;
            PubScript = pkScript;

            maxAmount = 21_000_000_0000_0000;
        }



        /// <summary>
        /// Minimum size of a TxOut. Amount(8) + CompactInt(1) + Script(0)
        /// </summary>
        internal int MinSize = sizeof(ulong) + 1;
        private readonly ulong maxAmount;

        public ulong Amount { get; set; }
        public IPubkeyScript PubScript { get; set; }


        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <returns>An array of bytes.</returns>
        public byte[] Serialize()
        {
            if (PubScript == null)
                throw new ArgumentNullException(nameof(PubScript), $"PubKey script can not be null.");
            if (Amount > maxAmount)
                throw new ArgumentOutOfRangeException(nameof(Amount), "Can not spend more than coin's maximum supply.");


            return ByteArray.ConcatArrays(
                Amount.ToByteArray(false),
                PubScript.Serialize()
                );
        }


        /// <summary>
        /// Deserializes the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing a <see cref="TxOut"/>.</param>
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
            if (data == null || data.Length - offset < MinSize)
            {
                error = "Data length is not valid.";
                return false;
            }


            Amount = data.SubArray(offset, 8).ToUInt64(false);
            offset += 8;
            if (Amount > maxAmount)
            {
                error = "Amout exceeds maximum coin's supply.";
                return false;
            }

            if (!PubScript.TryDeserialize(data, ref offset, out error))
            {
                return false;
            }

            error = null;
            return true;
        }

    }
}
