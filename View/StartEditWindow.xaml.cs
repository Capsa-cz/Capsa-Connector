using System;
using System.Windows;

namespace Capsa_Connector.View;

public partial class StartEditWindow : Window
{
    // On get just get, on set it will update description text
    public string? FileName
    {
        get => descriptions.Text;
        set
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                descriptions.Text = $"Soubor {value ?? ""} se právě otevírá.";
            });
        }
    }

    public StartEditWindow(string fileName)
    {
        InitializeComponent();
        FileName = fileName;
        this.Left = SystemParameters.PrimaryScreenWidth/2 - this.Width/2;
        this.Top = SystemParameters.PrimaryScreenHeight/2 - this.Height/2;
    }
}