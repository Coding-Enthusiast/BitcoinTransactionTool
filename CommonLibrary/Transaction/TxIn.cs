using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using System;

namespace CommonLibrary.Transaction
{
    public class TxIn
    {
        private Outpoint _outpoint;
        public Outpoint Outpoint { get => _outpoint; set => _outpoint = value; }

        /// <summary>
        /// ScriptSigLength is CompactSize but the max value is 10,000 bytes.
        /// </summary>
        public CompactInt ScriptSigLength { get; set; }
        public string ScriptSig { get; set; }
        public uint Sequence { get; set; }


        public int GetSize()
        {
            return Serialize().Length;
        }

        public byte[] Serialize()
        {
            return ByteArray.ConcatArrays(
                Outpoint.Serialize(),
                ScriptSigLength.Bytes,
                Base16.ToByteArray(ScriptSig),
                Sequence.ToByteArray(false)
                );
        }

        public static bool TryParse(byte[] ba, out TxIn result)
        {
            result = new TxIn();
            try
            {
                int index = 0;
                if (Outpoint.TryParse(ba.SubArray(index, Outpoint.Size), out result._outpoint))
                {
                    index += Outpoint.Size;

                    result.ScriptSigLength = new CompactInt(ba.SubArray(index, CompactInt.MaxSize));
                    index += result.ScriptSigLength.Bytes.Length;

                    result.ScriptSig = ba.SubArray(index, (int)result.ScriptSigLength.Number).ToBase16();
                    index += (int)result.ScriptSigLength.Number;

                    result.Sequence = ba.SubArray(index, 4).ToUInt32(false);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
