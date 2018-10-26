using CommonLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLibrary.CryptoEncoders
{
    /// <summary>
    /// A checksummed base32 format for native v0-16 witness outputs. 
    /// https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki
    /// </summary>
    public class Bech32
    {
        public Bech32()
        {

        }



        private const int Bech32MaxLength = 90;
        private const int CheckSumSize = 6;
        private const int HrpMinLength = 1;
        private const int HrpMaxLength = 83;
        private const int HrpMinValue = 33;
        private const int HrpMaxValue = 126;
        private const char Separator = '1';
        private const int DataMinLength = 6;
        private const string B32Chars = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        private readonly uint[] generator = { 0x3b6a57b2u, 0x26508e6du, 0x1ea119fau, 0x3d4233ddu, 0x2a1462b3u };



        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private uint Polymod(byte[] values)
        {
            uint chk = 1;
            foreach (var value in values)
            {
                uint b = chk >> 25;
                chk = ((chk & 0x1ffffff) << 5) ^ value;
                for (int i = 0; i < 5; i++)
                {
                    if (((b >> i) & 1) == 1)
                    {
                        chk ^= generator[i];
                    }
                }
            }
            return chk;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="hrp"></param>
        /// <returns></returns>
        private byte[] ExpandHrp(string hrp)
        {
            byte[] result = new byte[(2 * hrp.Length) + 1];
            for (int i = 0; i < hrp.Length; i++)
            {
                result[i] = (byte)(hrp[i] >> 5);
                result[i + hrp.Length + 1] = (byte)(hrp[i] & 31);
            }
            return result;
        }


        /// <summary>
        /// Performs a general power of 2 base conversion from a given base to a new base.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fromBits"></param>
        /// <param name="toBits"></param>
        /// <param name="pad"></param>
        /// <returns></returns>
        private byte[] ConvertBits(byte[] data, int fromBits, int toBits, bool pad = true)
        {
            int acc = 0;
            int bits = 0;
            int maxv = (1 << toBits) - 1;
            int maxacc = (1 << (fromBits + toBits - 1)) - 1;

            List<byte> result = new List<byte>();
            foreach (var b in data)
            {
                if ((b >> fromBits) > 0)
                {
                    return null;
                }
                acc = ((acc << fromBits) | b) & maxacc;
                bits += fromBits;
                while (bits >= toBits)
                {
                    bits -= toBits;
                    result.Add((byte)((acc >> bits) & maxv));
                }
            }
            if (pad)
            {
                if (bits > 0)
                {
                    result.Add((byte)((acc << (toBits - bits)) & maxv));
                }
            }
            else if (bits >= fromBits || (byte)((acc << (toBits - bits)) & maxv) != 0)
            {
                return null;
            }
            return result.ToArray();
        }


        /// <summary>
        /// Checks to see if a given string is a valid bech-32 encoded string.
        /// <para/>* Doesn't verify checksum.
        /// </summary>
        /// <param name="bech32EncodedString">Input string to check.</param>
        /// <returns>True if input was a valid bech-32 encoded string (without verifying checksum).</returns>
        public bool IsValidWithoutCheckSum(string bech32EncodedString)
        {
            if (string.IsNullOrEmpty(bech32EncodedString) || bech32EncodedString.Length > Bech32MaxLength)
            {
                return false;
            }

            // reject mixed upper and lower characters.
            if (bech32EncodedString.ToLower() != bech32EncodedString && bech32EncodedString.ToUpper() != bech32EncodedString)
            {
                return false;
            }

            int sepIndex = bech32EncodedString.LastIndexOf(Separator);
            if (sepIndex == -1) // no separator
            {
                return false;
            }

            string hrp = bech32EncodedString.Substring(0, sepIndex);
            if (hrp.Length < HrpMinLength || hrp.Length > HrpMaxLength ||
                !hrp.All(x => (byte)x >= HrpMinValue && (byte)x <= HrpMaxValue))
            {
                return false;
            }

            string data = bech32EncodedString.Substring(sepIndex + 1);
            if (data.Length < DataMinLength || !data.All(x => B32Chars.Contains(char.ToLower(x))))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bech32EncodedString"></param>
        /// <returns></returns>
        public bool IsValid(string bech32EncodedString)
        {
            if (!IsValidWithoutCheckSum(bech32EncodedString))
            {
                return false;
            }

            byte[] b32Arr = Bech32Decode(bech32EncodedString, out string hrp);

            return VerifyChecksum(hrp, b32Arr);
        }


        private byte[] Bech32Decode(string bech32EncodedString, out string hrp)
        {
            bech32EncodedString = bech32EncodedString.ToLower();

            int sepIndex = bech32EncodedString.LastIndexOf(Separator);
            hrp = bech32EncodedString.Substring(0, sepIndex);
            string data = bech32EncodedString.Substring(sepIndex + 1);

            byte[] b32Arr = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                b32Arr[i] = (byte)B32Chars.IndexOf(data[i]);
            }

            return b32Arr;
        }

        public byte[] Decode(string bech32EncodedString, out string hrp, out byte witVer)
        {
            if (!IsValidWithoutCheckSum(bech32EncodedString))
            {
                throw new ArgumentException("Input is not a valid bech32 encoded string.");
            }

            byte[] b32Arr = Bech32Decode(bech32EncodedString, out hrp);
            witVer = b32Arr[0];

            if (!VerifyChecksum(hrp, b32Arr))
            {
                throw new ArgumentException("Invalid checksum");
            }

            byte[] b256Arr = ConvertBits(b32Arr.SubArray(1, b32Arr.Length - CheckSumSize - 1), 5, 8, false);

            return b256Arr.SubArray(0, b256Arr.Length);
        }



        public string Encode(string hrp, byte witver, byte[] witprog)
        {
            byte[] b32Arr = ConvertBits(witprog, 8, 5, true).AppendToBeginning(witver);
            byte[] checksum = CalculateCheckSum(hrp, b32Arr);
            b32Arr = b32Arr.ConcatFast(checksum);

            StringBuilder result = new StringBuilder(b32Arr.Length);
            foreach (var b in b32Arr)
            {
                result.Append(B32Chars[b]);
            }

            return $"{hrp}{Separator}{result.ToString()}";
        }





        private byte[] CalculateCheckSum(string hrp, byte[] data)
        {
            // expand hrp, append data to it, and then add 6 zero bytes at the end.
            byte[] bytes = ExpandHrp(hrp).ConcatFast(data).ConcatFast(new byte[CheckSumSize]);

            // get polymod of the whole data and then flip the least significant bit.
            uint pm = Polymod(bytes) ^ 1;

            byte[] result = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                result[i] = (byte)((pm >> 5 * (5 - i)) & 31);
            }
            return result;
        }

        private bool VerifyChecksum(string hrp, byte[] data)
        {
            byte[] values = ExpandHrp(hrp).ConcatFast(data);
            return Polymod(values) == 1;
        }

    }
}
