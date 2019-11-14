// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Cryptography.Hashing;
using System;

namespace BitcoinTransactionTool.Backend.Blockchain.Scripts
{
    public class RedeemScript : Script, IRedeemScript
    {
        public RedeemScript()
        {
            IsWitness = false;
            OperationList = new IOperation[0];
            ScriptType = ScriptType.ScriptRedeem;
            hashFunc = new Ripemd160Sha256() /*coin.AddressHashFunction*/;

            witHashFunc = new Sha256(false); // TODO: set this field in ICoin
            maxLenOrCount = 10000; // TODO: set this to a real value and from ICoin
        }



        private IHashFunction hashFunc;
        private IHashFunction witHashFunc;



        public RedeemScriptType GetRedeemScriptType()
        {
            if (OperationList == null || OperationList.Length == 0)
            {
                return RedeemScriptType.Empty;
            }
            else if (OperationList.Length == 2 &&
                OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
                OperationList[1] is PushDataOp && ((PushDataOp)OperationList[1]).data.Length == hashFunc.HashByteSize)
            {
                return RedeemScriptType.P2SH_P2WPKH;
            }
            else if (OperationList.Length == 2 &&
                OperationList[0] is PushDataOp && OperationList[0].OpValue == OP._0 &&
                OperationList[1] is PushDataOp && ((PushDataOp)OperationList[1]).data.Length == witHashFunc.HashByteSize)
            {
                return RedeemScriptType.P2SH_P2WSH;
            }

            return RedeemScriptType.Unknown;
        }

        //public void SetToMultiSig(int m, int n, Tuple<PublicKey, bool>[] pubKeyList)
        //{
        //    if (m < 1 || m > 16 || m > n)
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

        //public void SetToP2SH_P2WPKH(PublicKey pubKey)
        //{
        //    byte[] hash = hashFunc.ComputeHash(pubKey.ToByteArray(true)); // Always use compressed
        //    OperationList = new IOperation[]
        //    {
        //        new PushDataOp(OP._0),
        //        new PushDataOp(hash)
        //    };
        //}

        public void SetToP2SH_P2WSH(Script witnessScript)
        {
            byte[] hash = witHashFunc.ComputeHash(witnessScript.ToByteArray());
            OperationList = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(hash)
            };
        }

        //public void SetToCheckLocktimeVerify(PublicKey pubKey, bool compressed, LockTime lt)
        //{
        //    OperationList = new IOperation[]
        //    {
        //        // TODO: change this to a better return type that doesn't have zeros
        //        new PushDataOp(lt.ToByteArray(false).TrimEnd()),
        //        new CheckLocktimeVerifyOp(),
        //        new DROPOp(),
        //        new PushDataOp(pubKey.ToByteArray(compressed)),
        //        new CheckSigOp()
        //    };
        //}

        public PubkeyScript ConvertP2WPKH_to_P2PKH()
        {
            if (GetRedeemScriptType() != RedeemScriptType.P2SH_P2WPKH)
            {
                throw new ArgumentException("This conversion only works for P2SH-P2WPKH redeem script types.");
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

            res.hashFunc = hashFunc;
            res.witHashFunc = witHashFunc;

            return res;
        }
    }
}
