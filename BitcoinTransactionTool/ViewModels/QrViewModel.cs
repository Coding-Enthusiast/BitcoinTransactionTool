using CommonLibrary;
using System.Windows.Media.Imaging;

namespace BitcoinTransactionTool.ViewModels
{
    public class QrViewModel : InpcBase
    {
        private BitmapImage qRCode;
        public BitmapImage QRCode
        {
            get { return qRCode; }
            set { SetField(ref qRCode, value); }
        }
    }
}
