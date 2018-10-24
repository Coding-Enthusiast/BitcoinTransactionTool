using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using System;
using System.Linq;

namespace CommonLibrary.Transaction
{
    public class Outpoint
    {
        /// <summary>
        /// 32 byte Tx hash + 4 byte out index
        /// </summary>
        public const int Size = 36;

        /// <summary>
        /// Tx Id is stored in internal byte order.
        /// </summary>
        public string TxId { get; set; }
        public uint Index { get; set; }


        public byte[] Serialize()
        {
            return ByteArray.ConcatArrays(
                Base16.ToByteArray(TxId).Reverse().ToArray(),
                Index.ToByteArray(false)
                );
        }


        public static bool TryParse(byte[] data, out Outpoint result)
        {
            result = new Outpoint();
            try
            {
                int index = 0;
                result.TxId = data.SubArray(0, 32).Reverse().ToArray().ToBase16();
                index += 32;

                result.Index = data.SubArray(index, 4).ToUInt32(false);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
