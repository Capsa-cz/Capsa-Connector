using Capsa_Connector.Core.Bases;

namespace Capsa_Connector.ViewModel
{
    class ConsoleViewModel : ViewModelBase
    {

        public RelayCommand ClearConsole { get; set; }

        private string consoleText;
        public string ConsoleText
        {
            get { return consoleText; }
            set
            {
                consoleText = value;
                OnPropertyChanged(nameof(ConsoleText));
            }
        }

        public ConsoleViewModel()
        {
            consoleText = "";
            ClearConsole = new RelayCommand((o) =>
            {
                ConsoleText = "";
            });
        }

        public void clearConsole()
        {
            ConsoleText = "";
        }
    }
}