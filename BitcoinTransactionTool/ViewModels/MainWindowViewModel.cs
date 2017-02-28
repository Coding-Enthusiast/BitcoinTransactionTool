using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;
using CommonLibrary;

namespace BitcoinTransactionTool.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {
            // Initializing the lists.
            ApiList = new ObservableCollection<ApiNames>(Enum.GetValues(typeof(ApiNames)).Cast<ApiNames>().ToList());
            SelectedApi = ApiNames.BlockchainInfo;

            WalletTypeList = new ObservableCollection<Transaction.WalletType>(Enum.GetValues(typeof(Transaction.WalletType)).Cast<Transaction.WalletType>().ToList());

            SendAddressList = new BindingList<SendingAddress>();
            UtxoList = new BindingList<UTXO>();
            ReceiveList = new BindingList<ReceivingAddress>();

            // Initializing Commands.
            GetUTXOCommand = AsyncCommand.Create(() => GetUTXO());
            MakeTxCommand = new BindableCommand(MakeTx, CanMakeTx);
            CopyTxCommand = new RelayCommand(CopyTx, () => !string.IsNullOrEmpty(RawTx));
            ShowQrWindowCommand = new RelayCommand(ShowQrWindow, () => !string.IsNullOrEmpty(RawTx));
            ShowJsonWindowCommand = new RelayCommand(ShowJsonWindow, () => !string.IsNullOrEmpty(RawTx));
            ShowEditWindowCommand = new RelayCommand(ShowEditWindow);

            // These moved below to avoid throwing null exception.
            ReceiveList.ListChanged += ReceiveList_ListChanged;
            SelectedUTXOs = new ObservableCollection<UTXO>();
            SelectionChangedCommand = new BindableCommand(SelectionChanged);
            SelectedWalletType = Transaction.WalletType.Normal;
        }


        #region Properties

        /// <summary>
        /// List of Api services used for receiving Unconfirmed Transaction Outputs (UTXO)
        /// </summary>
        public ObservableCollection<ApiNames> ApiList { get; set; }


        /// <summary>
        /// Api service that will be used to retreive UTXOs.
        /// </summary>
        public ApiNames SelectedApi
        {
            get { return selectedApi; }
            set { SetField(ref selectedApi, value); }
        }
        private ApiNames selectedApi;


        /// <summary>
        /// List of Bitcoin Addresses to receive their UTXOs and use for spending.
        /// </summary>
        public BindingList<SendingAddress> SendAddressList { get; set; }


        /// <summary>
        /// List of all UTXOs that can be used for spending.
        /// </summary>
        public BindingList<UTXO> UtxoList
        {
            get { return uTXOList; }
            set { SetField(ref uTXOList, value); }
        }
        private BindingList<UTXO> uTXOList;


        /// <summary>
        /// LockTime value used in transactions.
        /// </summary>
        public UInt32 LockTime
        {
            get { return lockTime; }
            set { SetField(ref lockTime, value); }
        }
        private UInt32 lockTime;


        /// <summary>
        /// Estimated size of the transaction based on number of inputs and outputs.
        /// </summary>
        [DependsOnProperty("SelectedUTXOs")]
        public int TransactionSize
        {
            get
            {
                int size = Transaction.GetTransactionSize(SelectedUTXOs.Count, ReceiveList.Count);
                return size;
            }
        }


        /// <summary>
        /// Sum of Sending Addresses balances which shows the total available amount to spend.
        /// </summary>
        [DependsOnProperty("UtxoList")]
        public decimal TotalBalance
        {
            get
            {
                return SendAddressList.Sum(x => x.Balance);
            }
        }


        /// <summary>
        /// Sum of selected UTXOs amounts, which is amount that is about to be spent.
        /// </summary>
        [DependsOnProperty("SelectedUTXOs")]
        public decimal TotalSelectedBalance
        {
            get
            {
                return SelectedUTXOs.Sum(x => x.AmountBitcoin);
            }
        }


        /// <summary>
        /// Total amount which is being sent to all the Receiving Addresses.
        /// </summary>
        public decimal TotalToSend
        {
            get
            {
                return ReceiveList.Sum(x => x.Payment);
            }
        }


        /// <summary>
        /// Amount of fee which is being paid (Must be >= 0).
        /// </summary>
        [DependsOnProperty(new string[] { "TotalSelectedBalance", "TotalToSend", "SelectedUTXOs" })]
        public decimal Fee
        {
            get
            {
                return TotalSelectedBalance - TotalToSend;
            }
        }


        /// <summary>
        /// Amount of fee in satoshi per byte based on estimated transaction size and fee amount.
        /// </summary>
        [DependsOnProperty("SelectedUTXOs")]
        public string FeePerByte
        {
            get
            {
                int size = 0;
                if (TransactionSize != 0)
                {
                    size = (int)(Fee / BitcoinConversions.Satoshi) / TransactionSize;
                }
                return string.Format("{0} satoshi/byte", size);
            }
        }


        /// <summary>
        /// List of selected UTXOs, these are the ones that will be spent.
        /// </summary>
        private ObservableCollection<UTXO> selectedUTXOs;
        public ObservableCollection<UTXO> SelectedUTXOs
        {
            get { return selectedUTXOs; }
            set
            {
                SetField(ref selectedUTXOs, value);

                MakeTxCommand.RaiseCanExecuteChanged();
            }
        }


        /// <summary>
        /// List of addresses to send coins to.
        /// </summary>
        public BindingList<ReceivingAddress> ReceiveList { get; set; }

        void ReceiveList_ListChanged(object sender, ListChangedEventArgs e)
        {
            RaisePropertyChanged("FeePerByte");
            RaisePropertyChanged("TotalToSend");
            RaisePropertyChanged("TransactionSize");

            MakeTxCommand.RaiseCanExecuteChanged();
        }


        /// <summary>
        /// Since Electrum (possible other wallets) only recognize particular scripts this is required.
        /// </summary>
        public ObservableCollection<Transaction.WalletType> WalletTypeList { get; set; }

        private Transaction.WalletType selectedWalletType;
        public Transaction.WalletType SelectedWalletType
        {
            get { return selectedWalletType; }
            set { SetField(ref selectedWalletType, value); }
        }


        /// <summary>
        /// Raw Unsigned Transaction result.
        /// </summary>
        private string rawTx;
        public string RawTx
        {
            get { return rawTx; }
            set
            {
                SetField(ref rawTx, value);

                CopyTxCommand.RaiseCanExecuteChanged();
                ShowQrWindowCommand.RaiseCanExecuteChanged();
                ShowJsonWindowCommand.RaiseCanExecuteChanged();
            }
        }


        /// <summary>
        /// An instance of the IWindowManager to show new views.
        /// </summary>
        private IWindowManager winManager;

        #endregion


        #region commands

        /// <summary>
        /// Handles getting the ListView.SelectedItems (multiple items)
        /// </summary>
        public BindableCommand SelectionChangedCommand { get; private set; }
        private void SelectionChanged(object param)
        {
            // param is of type System.Windows.Controls.SelectedItemCollection
            IList utxo = (IList)param;
            SelectedUTXOs = new ObservableCollection<UTXO>(utxo.Cast<UTXO>().ToList());
        }


        /// <summary>
        /// Contacts the selected Api service and receives the UTXO list.
        /// </summary>
        public IAsyncCommand GetUTXOCommand { get; private set; }
        private async Task GetUTXO()
        {
            IApiService api;
            switch (SelectedApi)
            {
                case ApiNames.BlockchainInfo:
                    api = new BlockchainInfoApi();
                    break;
                case ApiNames.BlockrIO:
                    api = new BlockrApi();
                    break;
                default:
                    api = new BlockchainInfoApi();
                    break;
            }

            UtxoList = new BindingList<UTXO>(await api.GetUTXO(SendAddressList.ToList()));
        }


        /// <summary>
        /// Creates the Raw Unsigned Transaction.
        /// </summary>
        public BindableCommand MakeTxCommand { get; private set; }
        private void MakeTx(object param)
        {
            // param is of type System.Windows.Controls.SelectedItemCollection
            IList utxo = (IList)param;
            RawTx = Transaction.CreateRawTx(utxo.Cast<UTXO>().ToList(), ReceiveList.ToList(), LockTime, SelectedWalletType);
        }
        private bool CanMakeTx()
        {
            if (SendAddressList.Count == 0 || SelectedUTXOs.Count == 0 || ReceiveList.Count == 0)
            {
                return false;
            }
            if (SendAddressList.Select(x => x.HasErrors).Contains(true) || ReceiveList.Select(x => x.HasErrors).Contains(true))
            {
                return false;
            }
            if (Fee < 0)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Builds QR code representing the Raw Transaction in a new window.
        /// </summary>
        public RelayCommand ShowQrWindowCommand { get; private set; }
        private void ShowQrWindow()
        {
            QrViewModel vm = new QrViewModel();
            vm.QRCode = TransactionQR.Build(RawTx);
            winManager = new QrWinManager();
            winManager.Show(vm);
        }


        /// <summary>
        /// Copies the created RawTx to clipboard.
        /// </summary>
        public RelayCommand CopyTxCommand { get; private set; }
        private void CopyTx()
        {
            Clipboard.SetText(RawTx);
        }


        /// <summary>
        /// Opens a new window to represent the RawTx as JSON string.
        /// </summary>
        public RelayCommand ShowJsonWindowCommand { get; private set; }
        private void ShowJsonWindow()
        {
            TxJsonViewModel vm = new TxJsonViewModel();
            vm.RawTx = RawTx;
            winManager = new TxJsonWinManager();
            winManager.Show(vm);
        }


        /// <summary>
        /// Opens a new Window for handling deserializaion of transactions and Pushing them to the bitcoin network if they are signed.
        /// </summary>
        public RelayCommand ShowEditWindowCommand { get; private set; }
        private void ShowEditWindow()
        {
            TransactionEditViewModel vm = new TransactionEditViewModel();
            vm.Tx = RawTx;
            winManager = new TxEditWinManager();
            winManager.Show(vm);
        }

        #endregion
    }
}
