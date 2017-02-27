using System.Windows;

using BitcoinTransactionTool.Views;
using CommonLibrary;

namespace BitcoinTransactionTool.Services
{
    public interface IWindowManager
    {
        void Show(CommonBase ViewModel);
    }

    public class TxEditWinManager : IWindowManager
    {
        public void Show(CommonBase ViewModel)
        {
            TransactionEditWindow myWin = new TransactionEditWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }

    public class QrWinManager : IWindowManager
    {
        public void Show(CommonBase ViewModel)
        {
            QrWindow myWin = new QrWindow();
            myWin.DataContext = ViewModel;
            myWin.Owner = Application.Current.MainWindow;
            myWin.ShowDialog();
        }
    }
}
