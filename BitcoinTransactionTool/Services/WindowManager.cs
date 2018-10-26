using BitcoinTransactionTool.Views;
using CommonLibrary;
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
}
