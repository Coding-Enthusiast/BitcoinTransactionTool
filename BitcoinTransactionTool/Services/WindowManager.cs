using System.Windows;

using BitcoinTransactionTool.Views;
using CommonLibrary;

namespace BitcoinTransactionTool.Services
{
    public interface IWindowManager
    {
        void Show(BindableBase ViewModel);
    }

    public class TxEditWinManager : IWindowManager
    {
        public void Show(BindableBase ViewModel)
        {
            TransactionEditWindow myWin = new TransactionEditWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }

    public class QrWinManager : IWindowManager
    {
        public void Show(BindableBase ViewModel)
        {
            QrWindow myWin = new QrWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }

    public class TxJsonWinManager : IWindowManager
    {
        public void Show(BindableBase ViewModel)
        {
            TxJsonWindow myWin = new TxJsonWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }
}
