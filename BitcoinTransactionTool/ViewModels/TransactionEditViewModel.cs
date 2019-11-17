// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend;
using BitcoinTransactionTool.Backend.Blockchain;
using BitcoinTransactionTool.Backend.Blockchain.Scripts;
using BitcoinTransactionTool.Backend.Blockchain.Scripts.Operations;
using BitcoinTransactionTool.Backend.Encoders;
using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;
using CommonLibrary;
using System;
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

        private WalletType _selectedWalletType;
        public WalletType SelectedWalletType
        {
            get { return _selectedWalletType; }
            set { SetField(ref _selectedWalletType, value); }
        }

        private string _rawTx;
        public string RawTx
        {
            get { return _rawTx; }
            set { SetField(ref _rawTx, value); }
        }

        private string _rawTx2;
        public string RawTx2
        {
            get { return _rawTx2; }
            set { SetField(ref _rawTx2, value); }
        }


        private string _txid;
        public string TxId
        {
            get => _txid;
            set => SetField(ref _txid, value);
        }


        private uint _ver;
        public uint Version
        {
            get => _ver;
            set => SetField(ref _ver, value);
        }


        private uint _lt;
        public uint LockTime
        {
            get => _lt;
            set => SetField(ref _lt, value);
        }

        private bool _rbf;
        public bool IsRBF
        {
            get => _rbf;
            set => SetField(ref _rbf, value);
        }

        private BindingList<ReceivingAddress> receiveList;
        public BindingList<ReceivingAddress> ReceiveList
        {
            get => receiveList ?? new BindingList<ReceivingAddress>();
            set => SetField(ref receiveList, value);
        }
        void ReceiveList_ListChanged(object sender, ListChangedEventArgs e)
        {
            MakeTxCommand.RaiseCanExecuteChanged();
        }

        private ObservableCollection<UTXO> _utxoList;
        public ObservableCollection<UTXO> UtxoList
        {
            get => _utxoList ?? new ObservableCollection<UTXO>();
            set
            {
                if (SetField(ref _utxoList, value))
                {
                    MakeTxCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private readonly TxBuilder txBuilder = new TxBuilder();

        public RelayCommand DecodeTxCommand { get; private set; }
        private void DecodeTx()
        {
            Transaction tx = new Transaction();
            byte[] data = Base16.ToByteArray(RawTx);
            int offset = 0;
            if (tx.TryDeserialize(data, ref offset, out string error))
            {
                TxId = tx.GetTransactionId();
                Version = (uint)tx.Version;
                LockTime = tx.LockTime;
                IsRBF = tx.TxInList.Any(x => x.Sequence != uint.MaxValue); // TODO: this is inaccurate.

                UtxoList = new ObservableCollection<UTXO>(tx.TxInList.Select(x => new UTXO(x.Outpoint.GetTxId(), x.Outpoint.Index)));

                ReceiveList = new BindingList<ReceivingAddress>(tx.TxOutList.Select(x => new ReceivingAddress(GetAddr(x.PubScript), x.Amount)).ToList());
            }
            else
            {
                MessageBox.Show(error);
            }
        }
        private string GetAddr(IPubkeyScript scr)
        {
            Address addr = new Address();
            switch (scr.GetPublicScriptType())
            {
                case PubkeyScriptType.P2PKH:
                    return addr.BuildP2PKH(((PushDataOp)scr.OperationList[2]).data);
                case PubkeyScriptType.P2SH:
                    return addr.BuildP2SH(((PushDataOp)scr.OperationList[1]).data);
                case PubkeyScriptType.P2WPKH:
                    return addr.BuildP2WPKH(((PushDataOp)scr.OperationList[1]).data);
                case PubkeyScriptType.P2WSH:
                    return addr.BuildP2WSH(((PushDataOp)scr.OperationList[1]).data);
                case PubkeyScriptType.Empty:
                case PubkeyScriptType.Unknown:
                case PubkeyScriptType.P2PK:
                case PubkeyScriptType.P2MS:
                case PubkeyScriptType.CheckLocktimeVerify:
                case PubkeyScriptType.RETURN:
                default:
                    return "Undefined.";
            }
        }

        public RelayCommand MakeTxCommand { get; private set; }
        private void MakeTx()
        {
            var tx = txBuilder.Build(Version, UtxoList.ToList(), ReceiveList.ToList(), LockTime);
            RawTx2 = tx.Serialize().ToBase16();
        }
        private bool CanMakeTx()
        {
            return UtxoList.Count != 0 && ReceiveList.Count != 0 && !ReceiveList.Select(x => x.HasErrors).Contains(true);
        }

    }
}
