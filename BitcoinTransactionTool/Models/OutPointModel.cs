using CommonLibrary;
using CommonLibrary.Transaction;

namespace BitcoinTransactionTool.Models
{
    public class OutpointModel : InpcBase
    {
        public OutpointModel(Outpoint o)
        {
            TxId = o.TxId;
            Index = o.Index;
        }


        public string TxId { get; set; }
        public uint Index { get; set; }
    }
}
