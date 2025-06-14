using System.Windows;
using System.Windows.Controls;

namespace Capsa_Connector.View
{
    /// <summary>
    /// Interakční logika pro ConsoleView.xaml
    /// </summary>
    public partial class ConsoleView : UserControl
    {
        public ConsoleView()
        {
            InitializeComponent();
        }

        private void ClearConsole(object sender, RoutedEventArgs e)
        {
            //MainViewModel.consoleVM.clearConsole();
        }
    }
}
