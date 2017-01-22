using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using CommonLibrary;
using BitcoinTransactionTool.Models;
using BitcoinTransactionTool.Services;

namespace BitcoinTransactionTool.ViewModels
{
    public class MainWindowViewModel : CommonBase
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
            ShowQrWindowCommand = new RelayCommand(ShowQrWindow, CanShowQr);
            ShowEditWindowCommand = new RelayCommand(ShowEditWindow);

            // These moved below to avoid throwing null exception.
            ReceiveList.ListChanged += ReceiveList_ListChanged;
            SelectedUTXOs = new ObservableCollection<UTXO>();
            SelectionChangedCommand = new BindableCommand(SelectionChanged);
            SelectedWalletType = Transaction.WalletType.Normal;
        }

        /// <summary>
        /// List of Api services used for receiving Unconfirmed Transaction Outputs (UTXO)
        /// </summary>
        public ObservableCollection<ApiNames> ApiList { get; set; }

        private ApiNames selectedApi;
        public ApiNames SelectedApi
        {
            get { return selectedApi; }
            set
            {
                if (selectedApi != value)
                {
                    selectedApi = value;
                    RaisePropertyChanged("SelectedApi");
                }
            }
        }


        /// <summary>
        /// List of Bitcoin Addresses to receive their UTXOs and use for spending.
        /// </summary>
        public BindingList<SendingAddress> SendAddressList { get; set; }


        /// <summary>
        /// List of all UTXOs that can be used for spending.
        /// </summary>
        private BindingList<UTXO> uTXOList;
        public BindingList<UTXO> UtxoList
        {
            get { return uTXOList; }
            set
            {
                if (uTXOList != value)
                {
                    uTXOList = value;
                    RaisePropertyChanged("UtxoList");
                    RaisePropertyChanged("TotalBalance");
                }
            }
        }


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


        /// <summary>
        /// Sum of Sending Addresses balances which shows the total available amount to spend.
        /// </summary>
        public decimal TotalBalance
        {
            get
            {
                return SendAddressList.Sum(x => x.Balance);
            }
        }

        /// <summary>
        /// Sum of selected UTXOs amounts.
        /// </summary>
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
        public decimal Fee
        {
            get
            {
                decimal totalAvailableFromUTXOs = SelectedUTXOs.Sum(x => x.AmountBitcoin);
                return totalAvailableFromUTXOs - TotalToSend;
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
                if (selectedUTXOs != value)
                {
                    selectedUTXOs = value;
                    RaisePropertyChanged("SelectedUTXOs");
                    RaisePropertyChanged("TotalSelectedBalance");
                    RaisePropertyChanged("Fee");
                    CanMakeTx();
                    // Raise button canExecute event
                    MakeTxCommand.RaiseCanExecuteChanged();
                }
            }
        }


        /// <summary>
        /// List of addresses to send coins to.
        /// </summary>
        public BindingList<ReceivingAddress> ReceiveList { get; set; }

        void ReceiveList_ListChanged(object sender, ListChangedEventArgs e)
        {
            RaisePropertyChanged("Fee");
            RaisePropertyChanged("TotalToSend");

            // Check
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
            set
            {
                if (selectedWalletType != value)
                {
                    selectedWalletType = value;
                    RaisePropertyChanged("SelectedWalletType");
                }
            }
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
                if (rawTx != value)
                {
                    rawTx = value;
                    RaisePropertyChanged("RawTx");
                    ShowEditWindowCommand.RaiseCanExecuteChanged();
                    ShowQrWindowCommand.RaiseCanExecuteChanged();
                }
            }
        }



        #region commands

        /// <summary>
        /// Handles getting the ListView.SelectedItems (multiple)
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
            IWindowManager winManager = new QrWinManager();
            winManager.Show(vm);
        }
        private bool CanShowQr()
        {
            if (string.IsNullOrEmpty(RawTx))
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Opens a new Window for handling deserializaion of transactions and Pushing them to the bitcoin network if they are signed.
        /// </summary>
        public RelayCommand ShowEditWindowCommand { get; private set; }
        private void ShowEditWindow()
        {
            TransactionEditViewModel vm = new TransactionEditViewModel();
            vm.Tx = RawTx;
            IWindowManager winManager = new TxEditWinManager();
            winManager.Show(vm);
        }

        #endregion
    }
}
