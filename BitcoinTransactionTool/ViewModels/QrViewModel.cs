using System.Windows.Media.Imaging;

using CommonLibrary;

namespace BitcoinTransactionTool.ViewModels
{
    public class QrViewModel : BindableBase
    {
        /// <summary>
        /// QR Code representing the Raw Transaction.
        /// </summary>
        private BitmapImage qRCode;
        public BitmapImage QRCode
        {
            get { return qRCode; }
            set
            {
                if (qRCode != value)
                {
                    qRCode = value;
                    RaisePropertyChanged("QRCode");
                }
            }
        }
    }
}
