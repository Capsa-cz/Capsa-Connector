using Capsa_Connector.Controller.Core;
using Capsa_Connector.Core;
using Capsa_Connector.Core.FileControl;
using Capsa_Connector.View;
using Capsa_Connector.ViewModel;
using NetSparkleUpdater;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Capsa_Connector.Controller.Tools;
using Capsa_Connector.Core.Bases;

namespace Capsa_Connector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(App app, SparkleUpdater sparkle)
        {
            StaticVariables.capsaUrl = Config.capsaUrlDefault;
            if (!string.IsNullOrEmpty(Settings1.Default.capsaUrl)) StaticVariables.capsaUrl = Settings1.Default.capsaUrl;

            InitializeComponent();
            MouseDown += Window_MouseDown;

            string manifestModuleName = System.Reflection.Assembly.GetEntryAssembly()!.ManifestModule.FullyQualifiedName;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(manifestModuleName);

            // Do stuff, nobody knows what, but it's important
            System.Windows.Controls.RadioButton[] radioBtns = { rb1, rb2, (System.Windows.Controls.RadioButton)rb3.Children[0] };
            
            var mainViewModel = new MainViewModel(radioBtns, app, sparkle);
            StaticVariables.mainViewModel = mainViewModel;
            DataContext = mainViewModel;

            new ProgramControl().StartProgram();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) Maximize.Content = "❐";
            else Maximize.Content = "▢";
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Default, it will just hide the window - not close it (it can be implemented in the future)
        }
        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            // For future use
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // For future use
        }
        public void OpenExitInAppDialog()
        {
            this.Show();
            this.Focus();
            if(StaticVariables.activeFiles.Count > 0)
            {
                this.exitInAppDialog.Visibility = Visibility.Visible;
                this.blurEffect.Radius = 10;
            }
            else App.Current.Shutdown();
        }

        private void StopTermination(object sender, RoutedEventArgs e)
        {
            this.exitInAppDialog.Visibility = Visibility.Hidden;
            this.blurEffect.Radius = 0;
        }

        private void Terminate(object sender, RoutedEventArgs e)
        {
            this.exitInAppDialog.Visibility = Visibility.Hidden;
            this.blurEffect.Radius = 0;
            
            // Stop all connections
            EndConnections.EndAllFileConnections();
            EndConnections.EndAllThreads();
            
            App.Current.Shutdown();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Open website with link https://winfsp.dev/
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://winfsp.dev/",
                UseShellExecute = true
            });
        }
    }
}
