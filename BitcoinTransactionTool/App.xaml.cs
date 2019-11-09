using System.Windows;
using System.Windows.Threading;

namespace BitcoinTransactionTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled catastrophic exception was thrown:\n{e.Exception.Message}",
                            "Handling Exception in App.xaml", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }
    }
}
