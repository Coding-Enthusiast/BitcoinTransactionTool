using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;
using CommonLibrary;
using CommonLibrary.CryptoEncoders;
using CommonLibrary.Extensions;
using CommonLibrary.Transaction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BitcoinTransactionTool.ViewModels
{
    /// <summary>
    /// This part is not yet complete.
    /// It needs more work to add recognizing TxStatus, adding Push feature
    /// and also option to receive input Tx values from Api to calculate Fees
    /// and also adding ability to edit the transaction and create a new Raw Unsigned Tx with the same inputs but different fee.
    /// </summary>
    public class TransactionEditViewModel : ViewModelBase
    {
        public TransactionEditViewModel()
        {
            WalletTypeList = new ObservableCollection<WalletType>(Enum.GetValues(typeof(WalletType)).Cast<WalletType>().ToList());
            SelectedWalletType = WalletType.Normal;
            ReceiveList.ListChanged += ReceiveList_ListChanged;
            DecodeTxCommand = new RelayCommand(DecodeTx);
            MakeTxCommand = new RelayCommand(MakeTx, CanMakeTx);
        }



        public ObservableCollection<WalletType> WalletTypeList { get; set; }

        private WalletType selectedWalletType;
        public WalletType SelectedWalletType
        {
            get { return selectedWalletType; }
            set { SetField(ref selectedWalletType, value); }
        }

        private string rawTx;
        public string RawTx
        {
            get { return rawTx; }
            set { SetField(ref rawTx, value); }
        }

        private string rawTx2;
        public string RawTx2
        {
            get { return rawTx2; }
            set { SetField(ref rawTx2, value); }
        }

        private TxModel trx;
        public TxModel Trx
        {
            get { return trx; }
            set { SetField(ref trx, value); }
        }

        private BindingList<ReceivingAddress> receiveList;
        public BindingList<ReceivingAddress> ReceiveList
        {
            get { return receiveList ?? new BindingList<ReceivingAddress>(); }
            set { SetField(ref receiveList, value); }
        }
        void ReceiveList_ListChanged(object sender, ListChangedEventArgs e)
        {
            MakeTxCommand.RaiseCanExecuteChanged();
        }


        public RelayCommand DecodeTxCommand { get; private set; }
        private void DecodeTx()
        {
            try
            {
                Trx = TxService.DecodeRawTx(rawTx);
                ReceiveList = new BindingList<ReceivingAddress>(Trx.TxOutList.Select(x => new ReceivingAddress() { Address = x.PkScript, Payment = x.Amount }).ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        public RelayCommand MakeTxCommand { get; private set; }
        private void MakeTx()
        {
            Transaction tx = new Transaction(
                trx.Version,
                trx.TxInCount,
                trx.TxInList.Select(x => new TxIn()
                {
                    Outpoint = new Outpoint() { Index = x.Outpoint.Index, TxId = x.Outpoint.TxId },
                    ScriptSig = x.ScriptSig,
                    ScriptSigLength = new CompactInt(x.ScriptSigLength),
                    Sequence = x.Sequence
                }).ToArray(),
                trx.TxOutCount,
                trx.TxOutList.ToArray(),
                trx.LockTime);

            RawTx2 = tx.Serialize().ToBase16();
        }
        private bool CanMakeTx()
        {
            if (Trx == null || ReceiveList.Select(x => x.HasErrors).Contains(true))
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
