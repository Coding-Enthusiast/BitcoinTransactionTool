using CommonLibrary;
using CommonLibrary.Transaction;

namespace BitcoinTransactionTool.Models
{
    public class TxInModel : InpcBase
    {
        public TxInModel(TxIn tx)
        {
            outpoint = new OutpointModel(tx.Outpoint);
            scriptSigLength = tx.ScriptSigLength.Number;
            scriptSig = tx.ScriptSig;
            sequence = tx.Sequence;
        }


        private OutpointModel outpoint;
        public OutpointModel Outpoint
        {
            get { return outpoint; }
            set { SetField(ref outpoint, value); }
        }

        private ulong scriptSigLength;
        public ulong ScriptSigLength
        {
            get { return scriptSigLength; }
            set { SetField(ref scriptSigLength, value); }
        }

        private string scriptSig;
        public string ScriptSig
        {
            get { return scriptSig; }
            set { SetField(ref scriptSig, value); }
        }

        private uint sequence;
        public uint Sequence
        {
            get { return sequence; }
            set { SetField(ref sequence, value); }
        }

    }
}
