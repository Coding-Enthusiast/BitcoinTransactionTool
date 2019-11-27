// Bitcoin Transaction Tool
// Copyright (c) 2017 Coding Enthusiast
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BitcoinTransactionTool.Backend.MVVM;
using BitcoinTransactionTool.Views;
using System.Windows;

namespace BitcoinTransactionTool.Services
{
    public interface IWindowManager
    {
        void Show(InpcBase ViewModel);
    }

    public class TxEditWinManager : IWindowManager
    {
        public void Show(InpcBase ViewModel)
        {
            TransactionEditWindow myWin = new TransactionEditWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }

    public class QrWinManager : IWindowManager
    {
        public void Show(InpcBase ViewModel)
        {
            QrWindow myWin = new QrWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }

    public class TxJsonWinManager : IWindowManager
    {
        public void Show(InpcBase ViewModel)
        {
            TxJsonWindow myWin = new TxJsonWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }

    public class ScriptWinManager : IWindowManager
    {
        public void Show(InpcBase ViewModel)
        {
            ScriptWindow myWin = new ScriptWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }
}
