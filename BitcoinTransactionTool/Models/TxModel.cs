using CommonLibrary;
using CommonLibrary.Transaction;
using System.Collections.ObjectModel;

namespace BitcoinTransactionTool.Models
{
    public class TxModel : InpcBase
    {
        private bool? isSigned;
        public bool? IsSigned
        {
            get { return isSigned; }
            set { SetField(ref isSigned, value); }
        }

        private bool isRbf;
        public bool IsRbf
        {
            get { return isRbf; }
            set { SetField(ref isRbf, value); }
        }

        private string txId;
        public string TxId
        {
            get { return txId; }
            set { SetField(ref txId, value); }
        }

        private string wtxId;
        public string WtxId
        {
            get { return wtxId; }
            set { SetField(ref wtxId, value); }
        }

        private uint version;
        public uint Version
        {
            get { return version; }
            set { SetField(ref version, value); }
        }

        private ulong txInCount;
        public ulong TxInCount
        {
            get { return txInCount; }
            set { SetField(ref txInCount, value); }
        }

        public ObservableCollection<TxInModel> TxInList { get; set; }

        private ulong txOutCount;
        public ulong TxOutCount
        {
            get { return txOutCount; }
            set { SetField(ref txOutCount, value); }
        }

        public ObservableCollection<TxOut> TxOutList { get; set; }

        private uint lockTime;
        public uint LockTime
        {
            get { return lockTime; }
            set { SetField(ref lockTime, value); }
        }

    }
}
