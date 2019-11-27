// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Backend.MVVM;
using Newtonsoft.Json;
using System;

namespace BitcoinTransactionTool.ViewModels
{
    public class TxJsonViewModel : InpcBase
    {
        public TxJsonViewModel()
        {

        }



        /// <summary>
        /// String representing the raw transaction in hex format.
        /// </summary>
        public string RawTx
        {
            get { return _rawTx; }
            set
            {
                if (SetField(ref _rawTx, value))
                {
                    SetJson();
                }
            }
        }
        private string _rawTx;


        /// <summary>
        /// Transaction in Json formatet string
        /// </summary>
        public string TxJson
        {
            get { return _txJson; }
            set { SetField(ref _txJson, value); }
        }
        private string _txJson;



        internal static JsonSerializerSettings jSetting = new JsonSerializerSettings
        {
            Converters = { new ByteArrayHexConverter(true), new TransactionLocktimeConverter(), new TransactionScriptConverter() },
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };
        private void SetJson()
        {
            if (!Base16.IsValid(_rawTx))
            {
                TxJson = "Invalid hex.";
            }
            else
            {
                Transaction tx = new Transaction();
                int offset = 0;

                TxJson = tx.TryDeserialize(Base16.ToByteArray(_rawTx), ref offset, out string error)
                    ? JsonConvert.SerializeObject(tx, Formatting.Indented, jSetting)
                    : "Error while deserializing transaction:" + Environment.NewLine + error;
            }
        }
    }
}
