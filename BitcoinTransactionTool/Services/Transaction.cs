using System;
using System.Collections.Generic;
using System.Text;

using BitcoinTransactionTool.Models;
using CommonLibrary;

namespace BitcoinTransactionTool.Services
{
    public class Transaction
    {
        public enum WalletType
        {
            Normal,
            Core,
            Electrum
        }



        /// <summary>
        /// Reverse a transaction hash.
        /// </summary>
        /// <param name="hex">Hash of the transaction to reverse.</param>
        /// <returns></returns>
        private static string ReverseTx(string hex)
        {
            // Convert to byte[] and reverse
            byte[] ba = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                ba[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            // Reverse and Convert back to hex.
            Array.Reverse(ba);
            string result = BitConverter.ToString(ba);
            return result.Replace("-", "").ToLower();
        }


        /// <summary>
        /// Makes ScriptSig based on wallet type.
        /// </summary>
        /// <param name="hash">Hes string to put inside of ScriptSig.</param>
        /// <param name="type">Type of wallet that is supposed to sign this transaction.</param>
        /// <returns>ScriptSig</returns>
        public static string BuildScript(string hash, WalletType type)
        {
            string result = "";
            if (type == WalletType.Normal)
            {
                //76 a9 14 - 88 ac
                result =
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_DUP, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_HASH160, 1) +
                    NumberConversions.IntToHex((UInt64)hash.Length / 2, 1) +
                    hash +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_EQUALVERIFY, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_CHECKSIG, 1);
            }
            else if (type == WalletType.Electrum)
            {
                //01 ff 16 fd 00
                result =
                    NumberConversions.IntToHex(0x01, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_INVALIDOPCODE, 1) +
                    NumberConversions.IntToHex(0x16, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_PUBKEYHASH, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_0, 1) +
                    hash;
            }
            else if (type == WalletType.Core)
            {
                result = "";
            }
            return result;
        }

        /// <summary>
        /// Creates a Raw Unsigned bitcoin transaction Transaction.
        /// <para/> Sources:
        /// <para/> 1. https://bitcoin.stackexchange.com/questions/32628/redeeming-a-raw-transaction-step-by-step-example-required
        /// <para/> 2. https://bitcoin.org/en/developer-reference#raw-transaction-format
        /// <para/> 3. https://bitcoin.org/en/developer-examples#simple-raw-transaction
        /// </summary>
        /// <param name="txToSpend">List of UTXOs to spend.</param>
        /// <param name="receiveAddr">List of receiving addresses and the amount to be paid to each.</param>
        /// <param name="wallet">Type of the wallet (Electrum does not recognize normal type of scriptSig placeholder).</param>
        /// <returns>Rat unsigned transaction string.</returns>
        public static string CreateRawTx(List<UTXO> txToSpend, List<ReceivingAddress> receiveAddr, UInt32 lockTime, WalletType wallet)
        {
            StringBuilder rawTx = new StringBuilder();

            // 1) 4 byte - version
            UInt32 version = 1;
            rawTx.Append(NumberConversions.IntToHex(version, 4));

            // 2) ? byte - tx_in count (compactSize uint)
            int txInCount = txToSpend.Count;
            rawTx.Append(NumberConversions.MakeCompactSize((UInt64)txInCount));

            for (int i = 0; i < txInCount; i++)
            {
                // 3) 32 byte - TX hash (reverse)
                string txToSend = txToSpend[i].TxHash;
                rawTx.Append(ReverseTx(txToSend));

                // 4) 4 byte - output Index
                UInt32 outputIndex = txToSpend[i].OutIndex;
                rawTx.Append(NumberConversions.IntToHex(outputIndex, 4));

                // 5) ? byte - scriptSig length (compactSize uint) (Maximum value is 10,000 bytes)
                //    Can be from 0 and up. Will be replaced by actual scriptSig with another length.
                //    Format can be different based on the client which will be used for signing.
                if (string.IsNullOrEmpty(txToSpend[i].AddressHash160) && wallet != WalletType.Core)
                {
                    throw new Exception(string.Format("No address was found. Set wallet type to Core."));
                }
                string scriptSig = BuildScript(txToSpend[i].AddressHash160, wallet);
                rawTx.Append(NumberConversions.IntToHex((UInt64)(scriptSig.Length / 2), 1));

                // 6) ? byte - scriptSig which is filled with scriptPubkey temporarily (20 is half the length of address hash160)
                rawTx.Append(scriptSig);

                //7) 4 byte - sequence - max is 0xffffffff - can change for RBF transactions
                // Ref: 
                // https://bitcoin.stackexchange.com/questions/2025/what-is-txins-sequence
                UInt32 sequence = UInt32.MaxValue;
                rawTx.Append(NumberConversions.IntToHex(sequence, 4));
            }

            //8) ? byte - tx_out count (compactSize uint)
            int txOutCount = receiveAddr.Count;
            rawTx.Append(NumberConversions.MakeCompactSize((UInt64)txOutCount));

            foreach (var item in receiveAddr)
            {
                //9) 8 byte - amout to transfer
                UInt64 amount = item.PaymentSatoshi;
                rawTx.Append(NumberConversions.IntToHex(amount, 8));

                //10) ? byte - pk_script length (compactSize uint)
                string itemHash = BitcoinConversions.Base58ToHash160(item.Address);
                string outputScript = BuildScript(itemHash, WalletType.Normal);
                rawTx.Append(NumberConversions.MakeCompactSize((UInt64)outputScript.Length / 2));

                //11) ? byte - pk_script 
                rawTx.Append(outputScript);
            }

            //12) 4 byte - lock time
            // * If less than 500 million, locktime is parsed as a block height. The transaction can be added to any block which has this height or higher.
            // * If greater than or equal to 500 million, locktime is parsed using the Unix epoch time format (the number of seconds elapsed since 1970-01-01T00:00 UTC—currently over 1.395 billion). The transaction can be added to any block whose block time is greater than the locktime.
            rawTx.Append(NumberConversions.IntToHex(lockTime, 4));

            return rawTx.ToString();
        }


        /// <summary>
        /// Deserialize the transaction hex string.
        /// </summary>
        /// <param name="tx">Transaction hex string</param>
        /// <returns>Bitcoin Transaction</returns>
        public static BitcoinTransaction DecodeRawTx(string tx)
        {
            BitcoinTransaction btx = new BitcoinTransaction();
            int index = 0;

            // 1) 4 byte - version
            string version = tx.Substring(index, 8);
            btx.Version = (UInt32)NumberConversions.HexToUInt(version);
            index += 8;

            // 2) ? byte - tx_in count (CompactSize uint)
            btx.TxInCount = NumberConversions.ReadCompactSize(tx, ref index);

            bool isSigned = true;
            bool isRbf = false;
            // Initialize the array
            btx.TxInList = new TxIn[btx.TxInCount];
            for (UInt64 i = 0; i < btx.TxInCount; i++)
            {
                TxIn temp = new TxIn();
                // 3) 32 byte - TX hash (reverse)
                temp.TxId = tx.Substring(index, 64);
                temp.TxId = ReverseTx(temp.TxId);
                index += 64;

                // 4) 4 byte - output Index
                string outIndex = tx.Substring(index, 8);
                temp.OutIndex = (UInt32)NumberConversions.HexToUInt(outIndex);
                index += 8;

                // 5) ? byte - scriptSig length (CompactSize uint) (Maximum value is 10,000 bytes)
                string scriptSigLength = tx.Substring(index, 2);
                temp.ScriptSigLength = (int)NumberConversions.ReadCompactSize(tx, ref index);

                // 6) ? byte - scriptSig or a placeholder for unsigned (can be empty too)
                temp.ScriptSig = tx.Substring(index, temp.ScriptSigLength * 2);
                index += temp.ScriptSigLength * 2;

                //7) 4 byte - sequence - max is 0xffffffff - can change for RBF transactions
                string sequence = tx.Substring(index, 8);
                temp.Sequence = (UInt32)NumberConversions.HexToUInt(sequence);
                index += 8;

                btx.TxInList[i] = temp;

                // Check to see if all the inputs are signed
                if (temp.ScriptSigLength <= 25)
                {
                    isSigned = false;
                }

                // Check for opt-in Replace By Fee
                if (temp.Sequence != UInt32.MaxValue)
                {
                    isRbf = true;
                }
            }
            // Set transaction sign and RBF status.
            btx.Status = (isSigned) ? BitcoinTransaction.TxStatus.Signed : BitcoinTransaction.TxStatus.Unsigned;
            btx.IsRbf = isRbf;

            //8) ? byte - tx_out count (compactSize uint)
            btx.TxOutCount = NumberConversions.ReadCompactSize(tx, ref index);

            // Initialize the array
            btx.TxOutList = new TxOut[btx.TxOutCount];
            for (UInt64 i = 0; i < btx.TxOutCount; i++)
            {
                TxOut temp = new TxOut();

                //9) 8 byte - amout to transfer
                string amount = tx.Substring(index, 16);
                temp.Amount = NumberConversions.HexToUInt(amount);
                index += 16;

                //10) ? byte - pk_script length (compactSize uint)
                string pkScriptLength = tx.Substring(index, 2);
                temp.PkScriptLength = (Int32)NumberConversions.HexToUInt(pkScriptLength);
                index += 2;

                //11) ? byte - pk_script 
                temp.PkScript = tx.Substring(index, temp.PkScriptLength * 2);
                index += temp.PkScriptLength * 2;

                btx.TxOutList[i] = temp;
            }

            //12) 4 byte - lock time
            string lockTime = tx.Substring(index, 8);
            btx.LockTime = (UInt32)NumberConversions.HexToUInt(lockTime);
            index += 8;

            // If the transaction is signed, then it has a TxId
            if (isSigned)
            {
                btx.TxId = BitcoinConversions.GetTxId(tx);
            }

            return btx;
        }


        /// <summary>
        /// Returns estimated value for the final signed transaction.
        /// </summary>
        /// <param name="inputCount">Number of inputs to spend</param>
        /// <param name="outputCount">Number of outputs to receive</param>
        /// <returns>Transaction size</returns>
        public static int GetTransactionSize(int inputCount, int outputCount)
        {
            int c1 = NumberConversions.MakeCompactSize((UInt64)inputCount).Length / 2;
            int c2 = NumberConversions.MakeCompactSize((UInt64)outputCount).Length / 2;

            int inputSize =
                32 //TX hash
                + 4 //output Index
                + 1 //scriptSig length
                + 138 //scriptSig sig
                + 4; //sequence
            int outputSize =
                8 //Amount
                + 1 //pk_script length
                + 25; //pk_script

            int totalSize =
                4 //Version
                + c1 //TxIn Count
                + inputSize * inputCount
                + c2 //TxOut Count
                + outputSize * outputCount
                + 4; //LockTime

            return totalSize;
        }
    }
}
