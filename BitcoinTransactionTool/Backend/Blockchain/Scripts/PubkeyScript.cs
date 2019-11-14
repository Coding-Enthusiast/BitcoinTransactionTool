// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Cryptography.Hashing;
using System;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    /// <summary>
    /// A script included in transaction outputs which sets the conditions that must be fulfilled for those outputs to be spent. 
    /// Also known as public key script, scriptPub or locking script.
    /// </summary>
    public class PubkeyScript : Script, IPubkeyScript
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PubkeyScript"/> using given parameters of the <see cref="ICoin"/>.
        /// </summary>
        public PubkeyScript()
        {
            IsWitness = false;
            OperationList = new IOperation[0];
            ScriptType = ScriptType.ScriptPub;

            addrManager = new Address();
            hashFunc = new Ripemd160Sha256();
            witHashFunc = new Sha256(false); 
            redeemScrBuilder = new RedeemScript();
            op_return_MaxSize = 80; 
            compPubKeyLength = (/*coin.Curve.NSizeInBits*/256 / 8) + 1;
            uncompPubKeyLength = ((compPubKeyLength - 1) * 2) + 1;
            minMultiPubCount = 0;
            maxMultiPubCount = 20;
            maxLenOrCount = 10000; 
        }



        private Address addrManager;
        internal IHashFunction hashFunc;
        internal IHashFunction witHashFunc;
        private int op_return_MaxSize;
        private RedeemScript redeemScrBuilder;
        private int compPubKeyLength, uncompPubKeyLength;
        private int minMultiPubCount, maxMultiPubCount;



        /// <summary>
        /// Returns <see cref="PubkeyScriptType"/> type of this instance if it was defined.
        /// </summary>
        /// <returns>The <see cref="PubkeyScriptType"/></returns>
        public PubkeyScriptType GetPublicScriptType()
        {
            if (OperationList == null || OperationList.Length == 0)
            {
                return PubkeyScriptType.Empty;
            }
            else if (OperationList.Length == 1 && OperationList[0] is ReturnOp) // TODO: check OP_Return operation count and whether it must be enforced to be 1
            {
                return PubkeyScriptType.RETURN;
            }
            else if (OperationList.Length == 2 &&
                OperationList[0] is PushDataOp &&
                OperationList[1] is CheckSigOp)
            {
                if (((PushDataOp)OperationList[0]).data.Length == compPubKeyLength ||
                    ((PushDataOp)OperationList[0]).data.Length == uncompPubKeyLength)
                {
                    return PubkeyScriptType.P2PK;
                }
            }
            else if (OperationList.Length == 5 &&
                OperationList[0] is DUPOp &&
                OperationList[1] is Hash160Op &&
                OperationList[2] is PushDataOp &&
                OperationList[3] is EqualVerifyOp &&
                OperationList[4] is CheckSigOp &&
                ((PushDataOp)OperationList[2]).data.Length == hashFunc.HashByteSize)
            {
                return PubkeyScriptType.P2PKH;
            }
            else if (OperationList.Length == 3 &&
                OperationList[0] is Hash160Op &&
                OperationList[1] is PushDataOp &&
                OperationList[2] is EqualOp &&
                ((PushDataOp)OperationList[1]).data.Length == hashFunc.HashByteSize)
            {
                return PubkeyScriptType.P2SH;
            }
            else if (OperationList.Length == 2 &&
                OperationList[0] is PushDataOp &&
                OperationList[1] is PushDataOp &&
                OperationList[0].OpValue == OP._0 &&
                ((PushDataOp)OperationList[1]).data.Length == hashFunc.HashByteSize)
            {
                return PubkeyScriptType.P2WPKH;
            }
            else if (OperationList.Length == 2 &&
                OperationList[0] is PushDataOp &&
                OperationList[1] is PushDataOp &&
                OperationList[0].OpValue == OP._0 &&
                ((PushDataOp)OperationList[1]).data.Length == witHashFunc.HashByteSize)
            {
                return PubkeyScriptType.P2WSH;
            }
            else if (OperationList.Length >= 4 && // OP_1 <pub> OP_1 OP_CheckMultiSig
                OperationList[0] is PushDataOp &&
                OperationList[OperationList.Length - 2] is PushDataOp &&
                OperationList[OperationList.Length - 1] is CheckMultiSigOp)
            {
                // TODO: check for values of PushData and count of pubkeys to be correct.
                return PubkeyScriptType.P2MS;
            }


            return PubkeyScriptType.Unknown;
        }



        ///// <summary>
        ///// Sets this script to a "pay to pubkey" script using the given <see cref="PublicKey"/>.
        ///// </summary>
        ///// <exception cref="ArgumentNullException"/>
        ///// <param name="pubKey">Public key to use</param>
        ///// <param name="useCompressed">Determines whether to use compressed or uncompressed <see cref="PublicKey"/> format</param>
        //public void SetToP2PK(PublicKey pubKey, bool useCompressed)
        //{
        //    if (pubKey == null)
        //        throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");


        //    OperationList = new IOperation[]
        //    {
        //        new PushDataOp(pubKey.ToByteArray(useCompressed)),
        //        new CheckSigOp()
        //    };
        //}



        /// <summary>
        /// Sets this script to a "pay to pubkey hash" script using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2PKH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != hashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {hashFunc.HashByteSize} bytes long.");

            OperationList = new IOperation[]
            {
                new DUPOp(),
                new Hash160Op(),
                new PushDataOp(hash),
                new EqualVerifyOp(),
                new CheckSigOp()
            };
        }

        ///// <summary>
        ///// Sets this script to a "pay to pubkey hash" script using the given <see cref="PublicKey"/>.
        ///// </summary>
        ///// <exception cref="ArgumentNullException"/>
        ///// <param name="pubKey"><see cref="PublicKey"/> to use</param>
        ///// <param name="useCompressed">Indicates whether to use compressed or uncompressed <see cref="PublicKey"/> format</param>
        //public void SetToP2PKH(PublicKey pubKey, bool useCompressed)
        //{
        //    if (pubKey == null)
        //        throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

        //    byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(useCompressed));
        //    SetToP2PKH(hash);
        //}

        /// <summary>
        /// Sets this script to a "pay to pubkey hash" script using the given base-58 encoded address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2PKH (Base-58 encoded) address to use</param>
        public void SetToP2PKH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be empty or null.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2PKH, out byte[] hash))
            {
                throw new FormatException("Invalid P2PKH address.");
            }

            SetToP2PKH(hash);
        }



        /// <summary>
        /// Sets this script to a "pay to script hash" script using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2SH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != hashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {hashFunc.HashByteSize} bytes long.");


            OperationList = new IOperation[]
            {
                new Hash160Op(),
                new PushDataOp(hash),
                new EqualOp()
            };
        }

        /// <summary>
        /// Sets this script to a "pay to script hash" script using the given base-58 encoded P2SH address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2SH (Base-58 encoded) address to use</param>
        public void SetToP2SH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be empty or null.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2SH, out byte[] hash))
            {
                throw new FormatException("Invalid P2PSH address.");
            }

            SetToP2SH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to script hash" script using the given <see cref="RedeemScript"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">Redeem script to use</param>
        public void SetToP2SH(RedeemScript redeem)
        {
            if (redeem == null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");

            byte[] scrBytes = redeem.ToByteArray();
            byte[] hash = hashFunc.ComputeHash(scrBytes);
            SetToP2SH(hash);
        }



        ///// <summary>
        ///// Sets this script to a "pay to witness pubkey hash inside a pay to script hash" script 
        ///// using the given <see cref="PublicKey"/>.
        ///// </summary>
        ///// <exception cref="ArgumentNullException"/>
        ///// <param name="pubKey">Public key to use</param>
        //public void SetToP2SH_P2WPKH(PublicKey pubKey)
        //{
        //    if (pubKey == null)
        //        throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

        //    redeemScrBuilder.SetToP2SH_P2WPKH(pubKey);
        //    SetToP2SH(redeemScrBuilder);
        //}

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash inside a pay to script hash" script 
        /// using the given <see cref="RedeemScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">The <see cref="RedeemScript"/> to use</param>
        public void SetToP2SH_P2WPKH(RedeemScript redeem)
        {
            if (redeem == null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            // For SetToP2SH the redeem type doesn't matter. 
            // But if this functions is called we check to see if it is called with a correct type to be explicit.
            if (redeem.GetRedeemScriptType() != RedeemScriptType.P2SH_P2WPKH)
                throw new ArgumentException("Invalid redeem script type.");

            SetToP2SH(redeem);
        }



        /// <summary>
        /// Sets this script to a "pay to witness script hash inside a pay to script hash" script 
        /// using the given <see cref="RedeemScript"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="redeem">The <see cref="RedeemScript"/> to use</param>
        public void SetToP2SH_P2WSH(RedeemScript redeem)
        {
            if (redeem == null)
                throw new ArgumentNullException(nameof(redeem), "Redeem script can not be null.");
            // For SetToP2SH the redeem type doesn't matter. 
            // But if this functions is called we check to see if it is called with a correct type to be explicit.
            if (redeem.GetRedeemScriptType() != RedeemScriptType.P2SH_P2WSH)
                throw new ArgumentException("Invalid redeem script type.");

            SetToP2SH(redeem);
        }



        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script using the given public key hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash of the public key to use</param>
        public void SetToP2WPKH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != hashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {hashFunc.HashByteSize} bytes long.");


            OperationList = new IOperation[]
            {
                // TODO: OP_0 is the version and can be changed in future. 20 bytes hash size is also for version 0
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }

        ///// <summary>
        ///// Sets this script to a "pay to witness pubkey hash" script using the given <see cref="PublicKey"/>.
        ///// </summary>
        ///// <exception cref="ArgumentNullException"/>
        ///// <param name="pubKey">Public key to use</param>
        //public void SetToP2WPKH(PublicKey pubKey)
        //{
        //    if (pubKey == null)
        //        throw new ArgumentNullException(nameof(pubKey), "Pubkey can not be null.");

        //    byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(true)); // Compressed pubkey is always used.
        //    SetToP2WPKH(hash);
        //}

        /// <summary>
        /// Sets this script to a "pay to witness pubkey hash" script using the given address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">P2WPKH (native SegWit, Bech32 encoded) address to use</param>
        public void SetToP2WPKH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be empty or null.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2WPKH, out byte[] hash))
            {
                throw new FormatException("Invalid P2WPKH address.");
            }

            SetToP2WPKH(hash);
        }



        /// <summary>
        /// Sets this script to a "pay to witness script hash" script using the given hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="hash">Hash to use</param>
        public void SetToP2WSH(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (hash.Length != witHashFunc.HashByteSize)
                throw new ArgumentOutOfRangeException(nameof(hash), $"Hash must be {witHashFunc.HashByteSize} bytes long.");

            OperationList = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }

        /// <summary>
        /// Sets this script to a "pay to witness script hash" script using the given <see cref="Script"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="scr">Script to use</param>
        public void SetToP2WSH(Script scr)
        {
            if (scr == null)
                throw new ArgumentNullException(nameof(scr), "Witness script can not be null.");

            byte[] scrBa = scr.ToByteArray();
            byte[] hash = witHashFunc.ComputeHash(scrBa);
            SetToP2WSH(hash);
        }

        /// <summary>
        /// Sets this script to a "pay to witness script hash" script using the given address.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="address">Address to use</param>
        public void SetToP2WSH(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException(nameof(address), "Address can not be null or empty.");
            if (!addrManager.VerifyType(address, PubkeyScriptType.P2WSH, out byte[] hash))
                throw new FormatException("Invalid P2WSH address.");

            SetToP2WSH(hash);
        }


        
        //public void SetToP2MS(int m, int n, Tuple<PublicKey, bool>[] pubKeyList)
        //{
        //    // min is 0 and max is 20!
        //    if (m < minMultiPubCount || m > maxMultiPubCount || m > n)
        //        throw new ArgumentOutOfRangeException(nameof(m), "M must be between 1 and 16 and smaller than N.");
        //    if (n < 1 || n > 16)
        //        throw new ArgumentOutOfRangeException(nameof(n), "N must be between 1 and 16.");
        //    if (pubKeyList == null || pubKeyList.Length == 0)
        //        throw new ArgumentNullException(nameof(pubKeyList), "Pubkey list can not be null or empty.");
        //    if (pubKeyList.Length != n)
        //        throw new ArgumentOutOfRangeException(nameof(pubKeyList), $"Pubkey list must contain N (={n}) items.");

        //    OperationList = new IOperation[n + 3]; // OP_m | pub1 | pub2 | ... | pub(n) | OP_n | OP_CheckMultiSig
        //    OperationList[0] = new PushDataOp(m);
        //    OperationList[n + 1] = new PushDataOp(n);
        //    OperationList[n + 2] = new CheckMultiSigOp();
        //    int i = 1;
        //    foreach (var item in pubKeyList)
        //    {
        //        OperationList[i++] = new PushDataOp(item.Item1.ToByteArray(item.Item2));
        //    }
        //}


        //public void SetToCheckLocktimeVerify(LockTime lt, PublicKey pubKey, bool compressed)
        //{
        //    OperationList = new IOperation[]
        //    {
        //        new PushDataOp(lt.ToByteArray(false).TrimEnd()), // TODO: change this to a better return type that doesn't have zeros
        //        new CheckLocktimeVerifyOp(),
        //        new DROPOp(),
        //        new DUPOp(),
        //        new Hash160Op(),
        //        new PushDataOp(hashFunc.ComputeHash(pubKey.ToByteArray(compressed))),
        //        new EqualVerifyOp(),
        //        new CheckSigOp()
        //    };
        //}


        /// <summary>
        /// Sets this script to an unspendable "OP_RETURN" script using the given data.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">Byte array to use</param>
        public void SetToReturn(byte[] data)
        {
            if (data != null && data.Length > op_return_MaxSize)
                throw new ArgumentOutOfRangeException(nameof(data),
                    $"Data size is bigger than allowed OP_RETURN size (={op_return_MaxSize}");

            OperationList = new IOperation[]
            {
                new ReturnOp(data)
            };
        }

        /// <summary>
        /// Sets this script to an unspendable "OP_RETURN" script using the given script.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="scr">Script to use</param>
        public void SetToReturn(Script scr)
        {
            if (scr == null)
                throw new ArgumentNullException(nameof(scr), "Script can not be null.");

            byte[] data = scr.ToByteArray();
            SetToReturn(data);
        }


        public PubkeyScript ConvertP2WPKH_to_P2PKH()
        {
            if (GetPublicScriptType() != PubkeyScriptType.P2WPKH)
            {
                throw new ArgumentException("This conversion only works for P2WPKH script types.");
            }
            IOperation pushHash = OperationList[1];

            PubkeyScript res = new PubkeyScript()
            {
                OperationList = new IOperation[]
                {
                    new DUPOp(),
                    new Hash160Op(),
                    pushHash,
                    new EqualVerifyOp(),
                    new CheckSigOp()
                },
            };

            res.addrManager = addrManager;
            res.hashFunc = hashFunc;
            res.witHashFunc = witHashFunc;
            res.redeemScrBuilder = redeemScrBuilder;
            res.op_return_MaxSize = op_return_MaxSize;
            res.compPubKeyLength = compPubKeyLength;
            res.uncompPubKeyLength = uncompPubKeyLength;
            res.minMultiPubCount = minMultiPubCount;
            res.maxMultiPubCount = maxMultiPubCount;

            return res;
        }

    }
}
