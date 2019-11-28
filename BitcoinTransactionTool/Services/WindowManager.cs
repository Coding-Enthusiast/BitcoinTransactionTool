// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.MVVM;
using System.Windows;

namespace BitcoinTransactionTool.Services
{
    public interface IWindowManager
    {
        void Show(InpcBase ViewModel, string viewTitle);
    }


    public class WindowManager : IWindowManager
    {
        public void Show(InpcBase ViewModel, string viewTitle)
        {
            Window win = new Window()
            {
                Content = ViewModel,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                Title = viewTitle
            };

            win.ShowDialog();
        }
    }

}
