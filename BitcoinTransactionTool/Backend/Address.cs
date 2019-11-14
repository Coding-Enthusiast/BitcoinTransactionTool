// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Cryptography.Hashing;
using BitcoinTransactionTool.Backend.Encoders;
using System;

namespace BitcoinTransactionTool.Backend
{
    public class Address
    {
        public Address()
        {
            b58Encoder = new Base58();
            b32Encoder = new Bech32();
            hashFunc = new Ripemd160Sha256() /*coin.AddressHashFunction*/;
            witHashFunc = new Sha256(); // TODO: set this from ICoin

            versionByte_P2pkh_MainNet = 0;
            versionByte_P2pkh_TestNet = 111;
            versionByte_P2pkh_RegTest = 0;
            versionByte_P2sh_MainNet = 5;
            versionByte_P2sh_TestNet = 196;
            versionByte_P2sh_RegTest = 5;
            hrp_MainNet = "bc";
            hrp_TestNet = "tb";
            hrp_RegTest = "bcrt";
        }



        private readonly Base58 b58Encoder;
        private readonly Bech32 b32Encoder;
        private readonly IHashFunction hashFunc;
        private readonly IHashFunction witHashFunc;
        private readonly byte versionByte_P2pkh_MainNet, versionByte_P2pkh_TestNet, versionByte_P2pkh_RegTest;
        private readonly byte versionByte_P2sh_MainNet, versionByte_P2sh_TestNet, versionByte_P2sh_RegTest;
        private readonly string hrp_MainNet, hrp_TestNet, hrp_RegTest;

        public enum AddressType
        {
            /// <summary>
            /// Pay to Pubkey Hash
            /// </summary>
            P2PKH,

            /// <summary>
            /// Pay to Script Hash
            /// </summary>
            P2SH,

            /// <summary>
            /// Pay to Witness Public Key Hash
            /// </summary>
            P2WPKH,

            /// <summary>
            /// Pay to Witness Script Hash
            /// </summary>
            P2WSH
        }

        internal bool VerifyType(string address, PubkeyScriptType scrType, out byte[] result)
        {
            result = null;
            try
            {
                switch (scrType)
                {
                    case PubkeyScriptType.P2PKH:
                        byte[] decoded = b58Encoder.DecodeWithCheckSum(address);
                        if (decoded[0] != versionByte_P2pkh_MainNet &&
                            decoded[0] != versionByte_P2pkh_TestNet &&
                            decoded[0] != versionByte_P2pkh_RegTest &&
                            decoded.Length != hashFunc.HashByteSize)
                        {
                            return false;
                        }
                        result = decoded.SubArray(1);
                        return true;

                    case PubkeyScriptType.P2SH:
                        decoded = b58Encoder.DecodeWithCheckSum(address);
                        if (decoded[0] != versionByte_P2sh_MainNet &&
                            decoded[0] != versionByte_P2sh_TestNet &&
                            decoded[0] != versionByte_P2sh_RegTest &&
                            decoded.Length != hashFunc.HashByteSize)
                        {
                            return false;
                        }
                        result = decoded.SubArray(1);
                        return true;

                    case PubkeyScriptType.P2WPKH:
                        decoded = b32Encoder.Decode(address, out byte witVer, out string hrp);
                        if (witVer != 0 ||
                            decoded.Length != hashFunc.HashByteSize)
                        {
                            return false;
                        }
                        result = decoded;
                        return true;

                    case PubkeyScriptType.P2WSH:
                        decoded = b32Encoder.Decode(address, out witVer, out hrp);
                        if (witVer != 0 ||
                            decoded.Length != witHashFunc.HashByteSize)
                        {
                            return false;
                        }
                        result = decoded;
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal bool TryGetType(string address, out PubkeyScriptType scrType, out byte[] hash)
        {
            scrType = PubkeyScriptType.Unknown;
            if (string.IsNullOrWhiteSpace(address))
            {
                hash = null;
                return false;
            }

            try
            {
                if (b58Encoder.IsValid(address))
                {
                    byte[] decoded = b58Encoder.DecodeWithCheckSum(address);

                    if (decoded.Length != hashFunc.HashByteSize + 1)
                    {
                        hash = null;
                        return false;
                    }
                    else if (decoded[0] != versionByte_P2pkh_MainNet ||
                            decoded[0] != versionByte_P2pkh_TestNet ||
                            decoded[0] != versionByte_P2pkh_RegTest)
                    {
                        scrType = PubkeyScriptType.P2PKH;
                        hash = decoded.SubArray(1);
                        return true;
                    }
                    else if (decoded[0] != versionByte_P2sh_MainNet ||
                            decoded[0] != versionByte_P2sh_TestNet ||
                            decoded[0] != versionByte_P2sh_RegTest)
                    {
                        scrType = PubkeyScriptType.P2SH;
                        hash = decoded.SubArray(1);
                        return true;
                    }
                    else
                    {
                        hash = null;
                        return false;
                    }
                }
                else if (b32Encoder.IsValid(address))
                {
                    byte[] decoded = b32Encoder.Decode(address, out byte witVer, out string hrp);

                    if (witVer != 0)
                    {
                        hash = null;
                        return false;
                    }
                    else if (decoded.Length == hashFunc.HashByteSize &&
                            hrp == hrp_MainNet ||
                            hrp == hrp_TestNet ||
                            hrp == hrp_RegTest)
                    {
                        scrType = PubkeyScriptType.P2WPKH;
                        hash = decoded;
                        return true;
                    }
                    else if (decoded.Length == witHashFunc.HashByteSize &&
                            hrp == hrp_MainNet ||
                            hrp == hrp_TestNet ||
                            hrp == hrp_RegTest)
                    {
                        scrType = PubkeyScriptType.P2WPKH;
                        hash = decoded;
                        return true;
                    }
                    else
                    {
                        hash = null;
                        return false;
                    }
                }
            }
            catch (Exception) { }

            hash = null;
            return false;
        }
    }
}
