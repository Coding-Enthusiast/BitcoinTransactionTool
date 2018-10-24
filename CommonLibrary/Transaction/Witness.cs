using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using System;

namespace CommonLibrary.Transaction
{
    public class Witness
    {
        public CompactInt StackItemCount { get; set; }
        public StackItem[] StackItemList { get; set; }


        public int GetSize()
        {
            return Serialize().Length;
        }

        public byte[] Serialize()
        {
            return ByteArray.ConcatArrays(
                StackItemCount.Bytes,
                SerializeStackItems()
                );
        }
        private byte[] SerializeStackItems()
        {
            byte[] result = { };
            if (StackItemCount.Number == 0)
            {
                return result;
            }
            foreach (var item in StackItemList)
            {
                result = result.ConcatFast(item.Serialize());
            }
            return result;
        }

        public static bool TryParse(byte[] data, out Witness result)
        {
            result = new Witness();
            try
            {
                int index = 0;
                result.StackItemCount = new CompactInt(data.SubArray(index, CompactInt.MaxSize));
                index += result.StackItemCount.Bytes.Length;

                result.StackItemList = new StackItem[result.StackItemCount.Number];
                for (ulong i = 0; i < result.StackItemCount.Number; i++)
                {
                    if (StackItem.TryParse(data.SubArray(index, data.Length - index), out result.StackItemList[i]))
                    {
                        index += result.StackItemList[i].GetSize();
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }


    public class StackItem
    {
        public CompactInt Size { get; set; }
        public string Data { get; set; }


        public int GetSize()
        {
            return Serialize().Length;
        }

        public byte[] Serialize()
        {
            return ByteArray.ConcatArrays(
                Size.Bytes,
                Base16.ToByteArray(Data)
                );
        }

        public static bool TryParse(byte[] data, out StackItem result)
        {
            result = new StackItem();
            try
            {
                int index = 0;
                result.Size = new CompactInt(data.SubArray(index, CompactInt.MaxSize));
                index += result.Size.Bytes.Length;

                result.Data = data.SubArray(index, (int)result.Size.Number).ToBase16();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
