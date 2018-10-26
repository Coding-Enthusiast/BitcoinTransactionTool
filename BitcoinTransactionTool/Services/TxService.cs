using BitcoinTransactionTool.Models;
using CommonLibrary;
using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using CommonLibrary.Transaction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BitcoinTransactionTool.Services
{
    public enum WalletType
    {
        Normal,
        Core,
        Electrum
    }
    public class TxService
    {
        /// <summary>
        /// Makes ScriptSig based on wallet type.
        /// <para/> INCOMPLETE!
        /// </summary>
        /// <param name="addr">Hes string to put inside of ScriptSig.</param>
        /// <param name="type">Type of wallet that is supposed to sign this transaction.</param>
        /// <returns>ScriptSig</returns>
        public static string BuildScriptSig(string addr, WalletType type)
        {
            string hash160 = BitcoinConversions.Base58ToHash160(addr);
            switch (type)
            {
                case WalletType.Electrum:
                    //01 ff 16 fd 00
                    return ByteArray.ConcatArrays(
                        new byte[] { 1, (byte)OPCodes.OP_INVALIDOPCODE, 16, (byte)OPCodes.OP_PUBKEYHASH, 0 },
                        Base16.ToByteArray(hash160)
                        ).ToBase16();
                case WalletType.Normal:
                case WalletType.Core:
                default:
                    return BuildScriptPub(addr);
            }
        }
        private static string BuildScriptPub(string addr)
        {
            string hash160 = BitcoinConversions.Base58ToHash160(addr);
            return ByteArray.ConcatArrays(
                new byte[] { (byte)OPCodes.OP_DUP, (byte)OPCodes.OP_HASH160 },
                new byte[] { (byte)(hash160.Length / 2) },
                Base16.ToByteArray(hash160),
                new byte[] { (byte)OPCodes.OP_EQUALVERIFY, (byte)OPCodes.OP_CHECKSIG }).ToBase16();
        }

        /// <summary>
        /// Creates a Raw Unsigned bitcoin transaction Transaction.
        /// </summary>
        /// <param name="txToSpend">List of UTXOs to spend.</param>
        /// <param name="receiveAddr">List of receiving addresses and the amount to be paid to each.</param>
        /// <param name="wallet">Type of the wallet (Electrum does not recognize normal type of scriptSig placeholder).</param>
        /// <returns>Raw unsigned transaction string.</returns>
        public static string CreateRawTx(uint ver, List<UTXO> txToSpend, List<ReceivingAddress> receiveAddr, uint lockTime, WalletType wallet)
        {
            TxIn[] tIns = txToSpend.Select(x => new TxIn()
            {
                Outpoint = new Outpoint() { Index = x.OutIndex, TxId = x.TxHash },
                ScriptSig = BuildScriptSig(x.Address, wallet),
                ScriptSigLength = new CompactInt((ulong)BuildScriptSig(x.Address, wallet).Length / 2),
                Sequence = uint.MaxValue
            }).ToArray();

            TxOut[] tOuts = receiveAddr.Select(x => new TxOut()
            {
                Amount = x.PaymentSatoshi,
                PkScript = BuildScriptPub(x.Address),
                PkScriptLength = new CompactInt((ulong)BuildScriptPub(x.Address).Length / 2)
            }).ToArray();

            Transaction tx = new Transaction(ver, (ulong)txToSpend.Count, tIns, (ulong)receiveAddr.Count, tOuts, lockTime);

            return tx.Serialize().ToBase16();
        }

        /// <summary>
        /// Deserialize the transaction hex string.
        /// </summary>
        /// <param name="rawTx">Transaction hex string</param>
        /// <returns>Bitcoin Transaction</returns>
        public static TxModel DecodeRawTx(string rawTx)
        {
            if (Transaction.TryParse(Base16.ToByteArray(rawTx), out Transaction tx))
            {
                TxModel result = new TxModel()
                {
                    Version = tx.Version,
                    TxInCount = tx.TxInCount.Number,
                    TxInList = new ObservableCollection<TxInModel>(tx.TxInList.Select(x => new TxInModel(x))),
                    TxOutCount = tx.TxOutCount.Number,
                    TxOutList = new ObservableCollection<TxOut>(tx.TxOutList),
                    LockTime = tx.LockTime,
                    TxId = tx.GetTransactionID(),
                    WtxId = tx.GetWitnessTransactionID(),
                    IsRbf = tx.TxInList.Any(x => x.Sequence != uint.MaxValue)
                };

                return result;
            }
            throw new ArgumentException("Can not parse the given transaction!");
        }


        /// <summary>
        /// Returns estimated size of the final signed transaction.
        /// </summary>
        /// <param name="inputs">Transactions to spend.</param>
        /// <param name="outputs">New outputs to create.</param>
        /// <returns></returns>
        public static int GetEstimatedTransactionSize(UTXO[] inputs, ReceivingAddress[] outputs)
        {
            if (inputs?.Length == 0 || outputs?.Length == 0)
            {
                return 0;
            }

            int c1 = new CompactInt((ulong)inputs.Length).Bytes.Length;
            int c2 = new CompactInt((ulong)outputs.Length).Bytes.Length;

            int inputSize = 0;
            foreach (var item in inputs)
            {
                if (item.Address.StartsWith("1"))
                {
                    inputSize +=
                        32 //TX hash
                        + 4 //output Index
                        + 1 //scriptSig length
                        + 107 //scriptSig sig
                        + 4; //sequence
                }
                else if (item.Address.StartsWith("3"))
                {
                    inputSize +=
                        32 //TX hash
                        + 4 //output Index
                        + 1 //scriptSig length
                        + 351 //scriptSig sig
                        + 4; //sequence
                }
                else if (item.Address.StartsWith("bc"))
                {
                    inputSize +=
                        32 //TX hash
                        + 4 //output Index
                        + 1 //scriptSig length
                        + 61 //scriptSig sig
                        + 4; //sequence
                }
            }

            int outputSize = 0;
            foreach (var item in outputs)
            {
                if (string.IsNullOrEmpty(item.Address))
                {
                    return 0;
                }

                if (item.Address.StartsWith("1"))
                {
                    outputSize =
                        8 //Amount
                        + 1 //pk_script length
                        + 25; //pk_script
                }
                else if (item.Address.StartsWith("3"))
                {
                    outputSize =
                        8 //Amount
                        + 1 //pk_script length
                        + 23; //pk_script
                }
                else if (item.Address.StartsWith("bc"))
                {
                    outputSize =
                        8 //Amount
                        + 1 //pk_script length
                        + 25; //pk_script
                }
            }

            int totalSize =
                4 //Version
                + c1 //TxIn Count
                + inputSize
                + c2 //TxOut Count
                + outputSize
                + 4; //LockTime

            return (totalSize <= 10) ? 0 : totalSize;
        }

    }
}
