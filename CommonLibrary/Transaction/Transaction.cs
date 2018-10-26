using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using CommonLibrary.Hashing;
using System;
using System.Linq;

namespace CommonLibrary.Transaction
{
    public class Transaction
    {
        public Transaction()
        {
        }
        public Transaction(uint ver, ulong txInCount, TxIn[] txIns, ulong txOutCount, TxOut[] txOuts, uint lockTime)
        {
            Version = ver;
            TxInCount = new CompactInt(txInCount);
            TxInList = txIns;
            TxOutCount = new CompactInt(txOutCount);
            TxOutList = txOuts;
            LockTime = lockTime;

            Flag = 0;
        }
        public Transaction(uint ver, ulong txInCount, TxIn[] txIns, ulong txOutCount, TxOut[] txOuts, uint lockTime, Witness[] witnesses)
        {
            Version = ver;
            TxInCount = new CompactInt(txInCount);
            TxInList = txIns;
            TxOutCount = new CompactInt(txOutCount);
            TxOutList = txOuts;
            LockTime = lockTime;

            Flag = 1;
            WitnessList = witnesses;
        }


        public uint Version { get; set; }
        public CompactInt TxInCount { get; set; }
        public TxIn[] TxInList { get; set; }
        public CompactInt TxOutCount { get; set; }
        public TxOut[] TxOutList { get; set; }
        public uint LockTime { get; set; }

        public byte Flag { get; set; }
        public Witness[] WitnessList { get; set; }


        public string GetTransactionID()
        {
            byte[] bytesToHash = (Flag == 1) ? SerializeWithoutWitness() : Serialize();
            return HashFunctions.ComputeDoubleSha256(bytesToHash).Reverse().ToArray().ToBase16();
        }
        public string GetWitnessTransactionID()
        {
            return HashFunctions.ComputeDoubleSha256(Serialize()).Reverse().ToArray().ToBase16();
        }


        private byte[] SerializeWithoutWitness()
        {
            return ByteArray.ConcatArrays(
                Version.ToByteArray(false),
                TxInCount.Bytes,
                SerializeTxIns(),
                TxOutCount.Bytes,
                SerializeTxOuts(),
                LockTime.ToByteArray(false)
                );
        }

        public byte[] Serialize()
        {
            return ByteArray.ConcatArrays(
                Version.ToByteArray(false),
                GetFlag(),
                TxInCount.Bytes,
                SerializeTxIns(),
                TxOutCount.Bytes,
                SerializeTxOuts(),
                GetWitness(),
                LockTime.ToByteArray(false)
                );
        }
        private byte[] GetFlag()
        {
            if (Flag == 1)
            {
                return new byte[] { 0x00, 0x01 };
            }
            else
            {
                return new byte[0];
            }
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
        private byte[] GetWitness()
        {
            byte[] result = { };
            if (Flag == 1)
            {
                foreach (var w in WitnessList)
                {
                    result = result.ConcatFast(w.Serialize());
                }
            }
            return result;
        }

        public static bool TryParse(byte[] data, out Transaction result)
        {
            result = new Transaction();
            try
            {
                int index = 0;

                result.Version = data.SubArray(index, 4).ToUInt32(false);
                index += 4;

                if (data[index] == 0)
                {
                    result.Flag = data[index + 1];
                    index += 2;
                }

                result.TxInCount = new CompactInt(data.SubArray(index, CompactInt.MaxSize));
                index += result.TxInCount.Bytes.Length;

                result.TxInList = new TxIn[result.TxInCount.Number];
                for (ulong i = 0; i < result.TxInCount.Number; i++)
                {
                    if (TxIn.TryParse(data.SubArray(index, data.Length - index), out result.TxInList[i]))
                    {
                        index += result.TxInList[i].GetSize();
                    }
                    else
                    {
                        return false;
                    }
                }

                result.TxOutCount = new CompactInt(data.SubArray(index, CompactInt.MaxSize));
                index += result.TxOutCount.Bytes.Length;

                result.TxOutList = new TxOut[result.TxOutCount.Number];
                for (ulong i = 0; i < result.TxOutCount.Number; i++)
                {
                    if (TxOut.TryParse(data.SubArray(index, data.Length - index), out result.TxOutList[i]))
                    {
                        index += result.TxOutList[i].GetSize();
                    }
                    else
                    {
                        return false;
                    }
                }

                if (result.Flag == 1)
                {
                    result.WitnessList = new Witness[result.TxInCount.Number];
                    for (ulong i = 0; i < result.TxInCount.Number; i++)
                    {
                        if (Witness.TryParse(data.SubArray(index, data.Length - index), out result.WitnessList[i]))
                        {
                            index += result.WitnessList[i].GetSize();
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                result.LockTime = data.SubArray(index, 4).ToUInt32(false);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
