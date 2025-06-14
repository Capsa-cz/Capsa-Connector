using System.Windows;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.Model;

namespace Capsa_Connector.View;

public partial class UploadErrorWindow : Window
{
    ActiveFile activeFile;
    RelayCommand repeatCommand;
    RelayCommand saveCommand;
    public UploadErrorWindow(ActiveFile activeFile, RelayCommand repeatCommand, RelayCommand saveCommand)
    {
        InitializeComponent();
        Show();
        Activate();
        //Set to foreground
        Topmost = true;
        this.activeFile = activeFile;
        this.repeatCommand = repeatCommand;
        this.saveCommand = saveCommand;
    }


    private void Save_Click(object sender, RoutedEventArgs e)
    {
        saveCommand.Execute(new object());
    }

    private void Repeat_Click(object sender, RoutedEventArgs e)
    {
        repeatCommand.Execute(new object());
    }
}