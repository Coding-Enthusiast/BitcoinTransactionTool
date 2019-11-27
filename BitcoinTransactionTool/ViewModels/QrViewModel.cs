// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.MVVM;
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
