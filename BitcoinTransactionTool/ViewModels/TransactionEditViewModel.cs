using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;
using CommonLibrary;

namespace BitcoinTransactionTool.ViewModels
{
    /// <summary>
    /// This part is not yet complete.
    /// It needs more work to add recognizing TxStatus, adding Push feature
    /// and also option to receive input Tx values from Api to calculate Fees
    /// and also adding ability to edit the transaction and create a new Raw Unsigned Tx with the same inputs but different fee.
    /// </summary>
    public class TransactionEditViewModel : BindableBase
    {
        public TransactionEditViewModel()
        {
            DecodeTxCommand = new RelayCommand(DecodeTx, CanDecodeTx);
            MakeTxCommand = new RelayCommand(MakeTx, CanMakeTx);

            ReceiveList.ListChanged += ReceiveList_ListChanged;

            WalletTypeList = new ObservableCollection<Transaction.WalletType>(Enum.GetValues(typeof(Transaction.WalletType)).Cast<Transaction.WalletType>().ToList());
            SelectedWalletType = Transaction.WalletType.Normal;
        }



        private BitcoinTransaction bTx;
        public BitcoinTransaction BTx
        {
            get { return bTx; }
            set
            {
                if (bTx != value)
                {
                    bTx = value;
                    RaisePropertyChanged("BTx");
                }
            }
        }

        private string tx;
        public string Tx
        {
            get { return tx; }
            set
            {
                if (tx != value)
                {
                    tx = value;
                    RaisePropertyChanged("Tx");
                    DecodeTxCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private BindingList<TxIn> txInList;
        public BindingList<TxIn> TxInList
        {
            get { return txInList; }
            set
            {
                if (txInList != value)
                {
                    txInList = value;
                    RaisePropertyChanged("TxInList");
                }
            }
        }


        private BindingList<ReceivingAddress> receiveList;
        public BindingList<ReceivingAddress> ReceiveList
        {
            get
            {
                if (receiveList == null)
                {
                    receiveList = new BindingList<ReceivingAddress>();
                }
                return receiveList;
            }
            set
            {
                if (receiveList != value)
                {
                    receiveList = value;
                    RaisePropertyChanged("ReceiveList");
                }
            }
        }
        void ReceiveList_ListChanged(object sender, ListChangedEventArgs e)
        {
            MakeTxCommand.RaiseCanExecuteChanged();
        }


        private decimal totalInput;
        public decimal TotalInput
        {
            get { return totalInput; }
            set
            {
                if (totalInput != value)
                {
                    totalInput = value;
                    RaisePropertyChanged("TotalInput");
                }
            }
        }

        private decimal totalOutput;
        public decimal TotalOutput
        {
            get { return totalOutput; }
            set
            {
                if (totalOutput != value)
                {
                    totalOutput = value;
                    RaisePropertyChanged("TotalOutput");
                }
            }
        }

        private decimal fee;
        public decimal Fee
        {
            get { return fee; }
            set
            {
                if (fee != value)
                {
                    fee = value;
                    RaisePropertyChanged("Fee");
                }
            }
        }

        private string rawTx;
        public string RawTx
        {
            get { return rawTx; }
            set
            {
                if (rawTx != value)
                {
                    rawTx = value;
                    RaisePropertyChanged("RawTx");
                }
            }
        }

        /// <summary>
        /// Since Electrum (possible other wallets) only recognize particular scripts this is required.
        /// </summary>
        public ObservableCollection<Transaction.WalletType> WalletTypeList { get; set; }

        private Transaction.WalletType selectedWalletType;
        public Transaction.WalletType SelectedWalletType
        {
            get { return selectedWalletType; }
            set
            {
                if (selectedWalletType != value)
                {
                    selectedWalletType = value;
                    RaisePropertyChanged("SelectedWalletType");
                }
            }
        }



        public RelayCommand DecodeTxCommand { get; private set; }
        private void DecodeTx()
        {
            try
            {
                BTx = Transaction.DecodeRawTx(Tx);
                TxInList = new BindingList<TxIn>();
                foreach (var item in BTx.TxInList)
                {
                    TxInList.Add(item);
                }
                // The following commented line throws Collection Read Only when item is added to BindingList!
                // TxInList = new BindingList<TxIn>(bTx.TxInList);

                ReceiveList = new BindingList<ReceivingAddress>();
                foreach (var item in BTx.TxOutList)
                {
                    ReceivingAddress r = new ReceivingAddress();
                    r.Address = GetAddressFromScript(item.PkScript);
                    r.Payment = item.Amount * BitcoinConversions.Satoshi;
                    ReceiveList.Add(r);
                }

                TotalInput = 0;
                TotalOutput = BTx.TxOutList.Select(x => x.Amount).Aggregate((a, b) => a + b) / BitcoinConversions.Satoshi;
                Fee = 0;

                MakeTxCommand.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool CanDecodeTx()
        {
            if (string.IsNullOrWhiteSpace(Tx))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private string GetAddressFromScript(string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return string.Empty;
            }
            string firstOP = script.Substring(0, 2);
            string remove = string.Empty;
            if (firstOP == NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_DUP, 1))
            {
                remove =
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_DUP, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_HASH160, 1) +
                    NumberConversions.IntToHex(40 / 2, 1);
            }
            else if (firstOP == NumberConversions.IntToHex(0x01, 1))
            {
                remove =
                    NumberConversions.IntToHex(0x01, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_INVALIDOPCODE, 1) +
                    NumberConversions.IntToHex(0x16, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_PUBKEYHASH, 1) +
                    NumberConversions.IntToHex((int)OPCodes.opcodetype.OP_0, 1);
            }

            string addressHash = script.Substring(remove.Length, 40);
            return BitcoinConversions.Hash160ToBase58(addressHash);
        }


        /// <summary>
        /// Creates the Raw Unsigned Transaction.
        /// </summary>
        public RelayCommand MakeTxCommand { get; private set; }
        private void MakeTx()
        {
            List<UTXO> uList = new List<UTXO>();
            foreach (var item in TxInList)
            {
                UTXO u = new UTXO();
                u.TxHash = item.TxId;
                u.OutIndex = item.OutIndex;
                if (BTx.Status == BitcoinTransaction.TxStatus.Signed)
                {
                    int pubKeyLength = 65;
                    string pubKey = item.ScriptSig.Substring((item.ScriptSigLength * 2) - (pubKeyLength + 1));
                    u.Address = BitcoinConversions.PubKeyToBase58(pubKey);
                    u.AddressHash160 = BitcoinConversions.ByteArrayToHex(BitcoinConversions.PubKeyToHash160(pubKey));
                }
                else
                {
                    var addr = GetAddressFromScript(item.ScriptSig);
                    if (string.IsNullOrEmpty(addr))
                    {
                        u.Address = string.Empty;
                        u.AddressHash160 = string.Empty;
                    }
                    else
                    {
                        u.Address = addr;
                        u.AddressHash160 = BitcoinConversions.Base58ToHash160(addr);
                    }
                }

                uList.Add(u);
            }

            try
            {
                UInt32 lockTime = 0;
                RawTx = Transaction.CreateRawTx(uList, ReceiveList.ToList(), lockTime, SelectedWalletType);
            }
            catch (Exception ex)
            {
                RawTx = string.Empty;
                MessageBox.Show(ex.Message);
            }
        }
        private bool CanMakeTx()
        {
            if (BTx == null || ReceiveList.Select(x => x.HasErrors).Contains(true))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
