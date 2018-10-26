using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;
using CommonLibrary;
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
            get { return rawTx; }
            set
            {
                if (SetField(ref rawTx, value))
                {
                    SetJson();
                }
            }
        }
        private string rawTx;


        /// <summary>
        /// Transaction in Json formatet string
        /// </summary>
        public string TxJson
        {
            get { return txJson; }
            set { SetField(ref txJson, value); }
        }
        private string txJson;


        /// <summary>
        /// Converts Raw Transaction hex into a JSON string representation of the transaction.
        /// </summary>
        private void SetJson()
        {
            try
            {
                TxModel btx = TxService.DecodeRawTx(rawTx);
                TxJson = JsonConvert.SerializeObject(btx, Formatting.Indented);
            }
            catch (Exception ex)
            {
                TxJson = "Not a Valid Transaction hex" + Environment.NewLine + ex.ToString();
            }
        }
    }
}
