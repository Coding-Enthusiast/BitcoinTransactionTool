namespace BitcoinTransactionTool.Models
{
    public class UTXO
    {
        public string Address { get; set; }
        public string AddressHash160 { get; set; }
        public string TxHash { get; set; }
        public uint OutIndex { get; set; }
        public int Confirmation { get; set; }
        
        /// <summary>
        /// Amount of bitcoin in current UTXO in satoshi
        /// </summary>
        public ulong Amount { get; set; }

        /// <summary>
        /// Bitcoin amount in bitcoin (*10e-8) for user interface only
        /// </summary>
        public decimal AmountBitcoin
        {
            get
            {
                return Amount * 0.00000001m;
            }
        }
    }
}
