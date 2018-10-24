using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using System;

namespace CommonLibrary.Transaction
{
    public class TxOut
    {
        public ulong Amount { get; set; }

        /// <summary>
        /// PkScriptLength is CompactSize but the max value is 10,000 bytes.
        /// </summary>
        public CompactInt PkScriptLength { get; set; }
        public string PkScript { get; set; }


        public int GetSize()
        {
            return Serialize().Length;
        }

        public byte[] Serialize()
        {
            return ByteArray.ConcatArrays(
                Amount.ToByteArray(false),
                PkScriptLength.Bytes,
                Base16.ToByteArray(PkScript)
                );
        }

        public static bool TryParse(byte[] ba, out TxOut result)
        {
            result = new TxOut();
            try
            {
                int index = 0;
                result.Amount = ba.SubArray(index, 8).ToUInt64(false);
                index += 8;

                result.PkScriptLength = new CompactInt(ba.SubArray(index, CompactInt.MaxSize));
                index += result.PkScriptLength.Bytes.Length;

                result.PkScript = ba.SubArray(index, (int)result.PkScriptLength.Number).ToBase16();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
