// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain;
using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitcoinTransactionTool.Services
{
    public enum WalletType
    {
        Normal,
        Core,
        Electrum
    }
    public class TxBuilder
    {
        private IPubkeyScript BuildPubkeyScript(string address)
        {
            Address addr = new Address();
            if (!addr.TryGetType(address, out PubkeyScriptType scrType, out byte[] hash))
            {
                throw new FormatException();
            }

            PubkeyScript scr = new PubkeyScript();
            switch (scrType)
            {
                case PubkeyScriptType.P2PKH:
                    scr.SetToP2PKH(hash);
                    break;
                case PubkeyScriptType.P2SH:
                    scr.SetToP2SH(hash);
                    break;
                case PubkeyScriptType.P2WPKH:
                    scr.SetToP2WPKH(hash);
                    break;
                case PubkeyScriptType.P2WSH:
                    scr.SetToP2WSH(hash);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return scr;
        }

        public ITransaction Build(uint ver, List<UTXO> txToSpend, List<ReceivingAddress> receiveAddr, uint lockTime)
        {
            TxIn[] tIns = txToSpend.Select(x => new TxIn()
            {
                Outpoint = new Outpoint() { Index = x.OutIndex, TxHash = Base16.ToByteArray(Base16.Reverse(x.TxHash)) },
                SigScript = new SignatureScript(),
                Sequence = uint.MaxValue
            }).ToArray();

            TxOut[] tOuts = receiveAddr.Select(x => new TxOut()
            {
                Amount = x.PaymentSatoshi,
                PubScript = BuildPubkeyScript(x.Address)
            }).ToArray();

            Transaction tx = new Transaction((int)ver, tIns, tOuts, lockTime);
            return tx;
        }


        /// <summary>
        /// Returns estimated size of the final signed transaction.
        /// </summary>
        /// <param name="inputs">Transactions to spend.</param>
        /// <param name="outputs">New outputs to create.</param>
        /// <returns></returns>
        public int GetEstimatedTransactionSize(UTXO[] inputs, ReceivingAddress[] outputs)
        {
            if (inputs?.Length == 0 || outputs?.Length == 0)
            {
                return 0;
            }


            int c1 = new CompactInt(inputs.Length).ToByteArray().Length;
            int c2 = new CompactInt(outputs.Length).ToByteArray().Length;

            int outpointSize = new Outpoint().Size;

            int inputSize = 0;
            foreach (var item in inputs)
            {
                if (item.Address.StartsWith("1"))
                {
                    inputSize +=
                        outpointSize
                        + 1 //scriptSig length
                        + 107 //scriptSig sig
                        + 4; //sequence
                }
                else if (item.Address.StartsWith("3"))
                {
                    inputSize +=
                        outpointSize
                        + 1 //scriptSig length
                        + 351 //scriptSig sig
                        + 4; //sequence
                }
                else if (item.Address.StartsWith("bc"))
                {
                    inputSize +=
                        outpointSize
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
