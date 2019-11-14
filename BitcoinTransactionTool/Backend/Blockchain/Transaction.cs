// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Cryptography.Hashing;
using System;
using System.Linq;

namespace BitcoinTransactionTool.Backend.Blockchain
{
    public class Transaction : IDeserializable, ITransaction
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Transaction"/> using parameters of the <see cref="ICoin"/>.
        /// </summary>
        public Transaction()
        {
            SetFields();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Transaction"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ver">Version</param>
        /// <param name="txIns">List of inputs</param>
        /// <param name="txOuts">List of outputs</param>
        /// <param name="lt">LockTime</param>
        /// <param name="witnesses">List of witnesses (default is null).</param>
        public Transaction(int ver, TxIn[] txIns, TxOut[] txOuts, LockTime lt, WitnessScript[] witnesses = null)
        {
            if (ver < 0)
                throw new ArgumentOutOfRangeException(nameof(ver), "Version can not be negative.");
            if (txIns == null || txIns.Length == 0)
                throw new ArgumentNullException(nameof(txIns), "TxIns can not be null or empty.");
            if (txOuts == null || txOuts.Length == 0)
                throw new ArgumentNullException(nameof(txOuts), "TxOuts can not be null or empty.");
            SetFields();

            Version = ver;
            TxInList = txIns;
            TxOutList = txOuts;
            LockTime = lt;

            WitnessList = witnesses;
        }



        public int Version { get; set; }
        public TxIn[] TxInList { get; set; }
        public TxOut[] TxOutList { get; set; }
        public IWitnessScript[] WitnessList { get; set; }
        public LockTime LockTime { get => _lockTime; set => _lockTime = value; }


        private IHashFunction hashFunc;
        private bool isTxIdReversed;
        internal int MinSize;
        internal int MaxSize;
        private LockTime _lockTime;

        private void SetFields()
        {
            hashFunc = new Sha256(true)/*coin.DataHashFunction*/;
            isTxIdReversed = true/*coin.IsTxIdHashReverse*/;
            MinSize = 4 + 1 + new TxIn().MinSize + 1 + new TxOut().MinSize + 4;
            MaxSize = 4_000_000/*coin.MaxBlockSize*/;
        }


        public int GetTotalSize()
        {
            return Serialize().Length;
        }
        public int GetBaseSize()
        {
            return SerializeWithoutWitness().Length;
        }
        public int GetWeight()
        {
            return (GetBaseSize() * 3) + GetTotalSize();
        }
        public int GetVirtualSize()
        {
            return GetWeight() / 4;
        }


        /// <summary>
        /// Returns hash of this instance using the defined <see cref="IHashFunction"/>.
        /// </summary>
        /// <remarks>
        /// This is the value used in <see cref="Outpoint.TxHash"/>.
        /// </remarks>
        /// <returns>Hash digest</returns>
        public byte[] GetTransactionHash()
        {
            byte[] bytesToHash = SerializeWithoutWitness(); // Tx hash is always stripping witness
            return hashFunc.ComputeHash(bytesToHash);
        }

        /// <summary>
        /// Returns transaction ID of this instance encoded using base-16 encoding.
        /// </summary>
        /// <returns>Base-16 encoded transaction ID.</returns>
        public string GetTransactionId()
        {
            // TODO: verify if transaction is signed then give TX ID. an unsigned tx doesn't have a TX ID.
            byte[] hashRes = GetTransactionHash();
            return isTxIdReversed ? hashRes.Reverse().ToArray().ToBase16() : hashRes.ToBase16();
        }


        public string GetWitnessTransactionId()
        {
            // TODO: same as above (verify if signed)
            byte[] hashRes = hashFunc.ComputeHash(Serialize());
            return isTxIdReversed ? hashRes.Reverse().ToArray().ToBase16() : hashRes.ToBase16();
        }


        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <returns>An array of bytes.</returns>
        public byte[] Serialize()
        {
            if (TxInList == null || TxInList.Length == 0)
                throw new ArgumentNullException(nameof(TxInList), "TxIns can not be null or empty.");
            if (TxOutList == null || TxOutList.Length == 0)
                throw new ArgumentNullException(nameof(TxOutList), "TxOuts can not be null or empty.");


            CompactInt tinCount = new CompactInt(TxInList.Length);
            CompactInt toutCount = new CompactInt(TxOutList.Length);

            return ByteArray.ConcatArrays(
                Version.ToByteArray(false),
                (WitnessList != null && WitnessList.Length > 0) ? new byte[] { 0x00, 0x01 } : new byte[0],
                tinCount.ToByteArray(),
                SerializeTxIns(),
                toutCount.ToByteArray(),
                SerializeTxOuts(),
                SerializeWitnesses(),
                LockTime.ToByteArray(false)
                );
        }


        public byte[] SerializeWithoutWitness()
        {
            if (TxInList == null || TxInList.Length == 0)
                throw new ArgumentNullException(nameof(TxInList), "TxIns can not be null or empty.");
            if (TxOutList == null || TxOutList.Length == 0)
                throw new ArgumentNullException(nameof(TxOutList), "TxOuts can not be null or empty.");


            CompactInt tinCount = new CompactInt(TxInList.Length);
            CompactInt toutCount = new CompactInt(TxOutList.Length);

            byte[] tinArr = SerializeTxIns();
            byte[] toutArr = SerializeTxOuts();

            return ByteArray.ConcatArrays(
                Version.ToByteArray(false),
                tinCount.ToByteArray(),
                tinArr,
                toutCount.ToByteArray(),
                toutArr,
                LockTime.ToByteArray(false)
                );

        }



        private byte[] SerializeTxIns()
        {
            byte[] result = { };
            foreach (var tx in TxInList)
            {
                result = result.ConcatFast(tx.Serialize());
            }
            return result;
        }

        private byte[] SerializeTxOuts()
        {
            byte[] result = { };
            foreach (var tx in TxOutList)
            {
                result = result.ConcatFast(tx.Serialize());
            }
            return result;
        }

        private byte[] SerializeWitnesses()
        {
            byte[] result = { };
            if (WitnessList?.Length > 0)
            {
                foreach (var w in WitnessList)
                {
                    result = result.ConcatFast(w.Serialize());
                }
            }
            return result;
        }


        /// <summary>
        /// Deserializes the given byte array starting from the specified offset. The return value indicates success.
        /// </summary>
        /// <param name="data">Byte array containing a <see cref="Transaction"/>.</param>
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


            Version = data.SubArray(offset, sizeof(int)).ToInt32(false);
            offset += sizeof(int);

            bool hasWitness = false;
            if (data[offset] == 0)
            {
                if (data[offset + 1] != 1)
                {
                    error = "The SegWit marker has to be 0x0001";
                    return false;
                }
                hasWitness = true;
                offset += 2;
            }

            if (!CompactInt.TryReadFromBytes(data, ref offset, out CompactInt tinCount, out error))
            {
                return false;
            }
            if (tinCount.Value > int.MaxValue) // TODO: set a better value to check against.
            {
                error = "TxIn count is too big.";
                return false;
            }
            // TODO: (highly unlikely) add a check for when (tinCount * eachTinSize) overflows size of our data

            TxInList = new TxIn[(int)tinCount.Value];
            for (int i = 0; i < TxInList.Length; i++)
            {
                TxInList[i] = new TxIn();
                if (!TxInList[i].TryDeserialize(data, ref offset, out error))
                {
                    return false;
                }
            }

            if (!CompactInt.TryReadFromBytes(data, ref offset, out CompactInt toutCount, out error))
            {
                return false;
            }
            if (toutCount.Value > int.MaxValue) // TODO: set a better value to check against.
            {
                error = "TxOut count is too big.";
                return false;
            }
            // TODO: (highly unlikely) add a check for when (toutCount * eachToutSize) overflows size of our data

            TxOutList = new TxOut[toutCount.Value];
            for (int i = 0; i < TxOutList.Length; i++)
            {
                TxOutList[i] = new TxOut();
                if (!TxOutList[i].TryDeserialize(data, ref offset, out error))
                {
                    return false;
                }
            }

            if (hasWitness)
            {
                WitnessList = new WitnessScript[TxInList.Length];
                for (int i = 0; i < WitnessList.Length; i++)
                {
                    WitnessList[i] = new WitnessScript();
                    if (!WitnessList[i].TryDeserialize(data, ref offset, out error))
                    {
                        return false;
                    }
                }
            }
            else
            {
                WitnessList = null;
            }

            if (!LockTime.TryReadFromBytes(data, ref offset, out _lockTime, out error))
            {
                return false;
            }

            if (offset > MaxSize)
            {
                error = "Transaction is bigger than allowed maximum size.";
                return false;
            }

            error = null;
            return true;
        }

    }
}
