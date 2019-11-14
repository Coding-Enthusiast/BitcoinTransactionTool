// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using System;

namespace BitcoinTransactionTool.Backend.Blockchain
{
    public class TxIn : IDeserializable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TxIn"/> using parameters of the <see cref="ICoin"/>.
        /// </summary>
        public TxIn()
        {
            SetFields();
            Outpoint = new Outpoint();
            SigScript = new SignatureScript();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TxIn"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="op">Transaction output</param>
        /// <param name="sigScript">Signature script</param>
        /// <param name="seq">Sequence</param>
        public TxIn(Outpoint op, SignatureScript sigScript, uint seq)
        {
            SetFields();
            if (op == null)
                throw new ArgumentNullException(nameof(op), "Outpoint can not be null!");
            if (sigScript == null)
                throw new ArgumentNullException(nameof(sigScript), "Signature script can not be null!");

            Outpoint = op;
            SigScript = sigScript;
            Sequence = seq;
        }



        public Outpoint Outpoint { get; set; }
        public ISignatureScript SigScript { get; set; }
        // TODO: read this about sequence and probably create a new variable type for it:
        // https://github.com/bitcoin/bips/blob/master/bip-0068.mediawiki
        public uint Sequence { get; set; }

        internal int MinSize;
        internal int MaxSize;



        private void SetFields()
        {
            CompactInt temp = new CompactInt(10000 /*coin.ScriptSigMaxLength*/);
            int opSize = new Outpoint().Size;
            MinSize = opSize + 1 + 0 + sizeof(uint);
            MaxSize = opSize + temp.ToByteArray().Length + 10000 + sizeof(uint);
        }


        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>An array of bytes</returns>
        public byte[] Serialize()
        {
            if (Outpoint == null)
                throw new ArgumentNullException(nameof(Outpoint), "Outpoint can not be null.");
            if (SigScript == null)
                throw new ArgumentNullException(nameof(SigScript), "Signature script can not be null!");


            return ByteArray.ConcatArrays(
                Outpoint.Serialize(),
                SigScript.Serialize(),
                Sequence.ToByteArray(false)
                );
        }



        /// <summary>
        /// Deserializes the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing an <see cref="TxIn"/>.</param>
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
            // TODO: Outpoint and SigScript could be null, perform a check?

            if (!Outpoint.TryDeserialize(data, ref offset, out error))
            {
                return false;
            }

            if (!SigScript.TryDeserialize(data, ref offset, out error))
            {
                return false;
            }

            Sequence = data.SubArray(offset, sizeof(uint)).ToUInt32(false);
            offset += sizeof(uint);

            error = null;
            return true;
        }

    }
}
