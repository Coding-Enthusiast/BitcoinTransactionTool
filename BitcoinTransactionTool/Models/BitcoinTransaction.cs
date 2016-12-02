using System;

using CommonLibrary;

namespace BitcoinTransactionTool.Models
{
    public class TransactionBase : CommonBase
    {
        public enum TxStatus
        {
            Signed,
            Unsigned
        }

        private TxStatus? status;
        public TxStatus? Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    RaisePropertyChanged("Status");
                }
            }
        }

        private bool isRbf;
        public bool IsRbf
        {
            get { return isRbf; }
            set
            {
                if (isRbf != value)
                {
                    isRbf = value;
                    RaisePropertyChanged("IsRbf");
                }
            }
        }

        private string txId;
        public string TxId
        {
            get { return txId; }
            set
            {
                if (txId != value)
                {
                    txId = value;
                    RaisePropertyChanged("TxId");
                }
            }
        }
    }


    /// <summary>
    /// https://bitcoin.org/en/developer-reference#raw-transaction-format
    /// </summary>
    public class BitcoinTransaction : TransactionBase
    {
        private UInt32 version;
        public UInt32 Version
        {
            get { return version; }
            set
            {
                if (version != value)
                {
                    version = value;
                    RaisePropertyChanged("Version");
                }
            }
        }

        private UInt64 txInCount;
        public UInt64 TxInCount
        {
            get { return txInCount; }
            set
            {
                if (txInCount != value)
                {
                    txInCount = value;
                    RaisePropertyChanged("TxInCount");
                }
            }
        }

        public TxIn[] TxInList { get; set; }

        private UInt64 txOutCount;
        public UInt64 TxOutCount
        {
            get { return txOutCount; }
            set
            {
                if (txOutCount != value)
                {
                    txOutCount = value;
                    RaisePropertyChanged("TxOutCount");
                }
            }
        }

        public TxOut[] TxOutList { get; set; }

        private UInt32 lockTime;
        public UInt32 LockTime
        {
            get { return lockTime; }
            set
            {
                if (lockTime != value)
                {
                    lockTime = value;
                    RaisePropertyChanged("LockTime");
                }
            }
        }
    }

    /// <summary>
    /// https://bitcoin.org/en/developer-reference#txin
    /// </summary>
    public class TxIn
    {
        public string TxId { get; set; }
        public UInt32 OutIndex { get; set; }

        /// <summary>
        /// ScriptSigLength is CompactSize but the max value is 10,000 bytes.
        /// </summary>
        public int ScriptSigLength { get; set; }
        public string ScriptSig { get; set; }
        public UInt32 Sequence { get; set; }
    }

    /// <summary>
    /// https://bitcoin.org/en/developer-reference#txout
    /// </summary>
    public class TxOut
    {
        public UInt64 Amount { get; set; }

        /// <summary>
        /// PkScriptLength is CompactSize but the max value is 10,000 bytes.
        /// </summary>
        public int PkScriptLength { get; set; }
        public string PkScript { get; set; }
    }
}
