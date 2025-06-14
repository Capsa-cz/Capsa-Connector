using NetSparkleUpdater;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Capsa_Connector.View
{
    /// <summary>
    /// Interaction logic for DownloadUpdateWindow.xaml
    /// </summary>
    public partial class DownloadUpdateWindow : Window, IDownloadProgress
    {
        string _actionButtonTitleAfterDownload;
        public DownloadUpdateWindow(string downloadTitle, string actionButtonTitleAfterDownload)
        {
            Title = downloadTitle;
            _actionButtonTitleAfterDownload = actionButtonTitleAfterDownload;
            InitializeComponent();
        }

        public event DownloadInstallEventHandler DownloadProcessCompleted;

        void IDownloadProgress.Close()
        {
            Close();
        }

        bool IDownloadProgress.DisplayErrorMessage(string errorMessage)
        {
            //TbStatus.Text = errorMessage;
            return true;
        }

        void IDownloadProgress.FinishedDownloadingFile(bool isDownloadedFileValid)
        {
            if (isDownloadedFileValid)
            {
                Thread.Sleep(1500);
                DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
            }
            else
            {
                //TbStatus.Text = "Při stahování nastal problém";
            }
        }

        void IDownloadProgress.OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
        {
            PbDownloadProgress.Value = args.ProgressPercentage;
            TbDownloadProgressText.Text = $"{args.BytesReceived / (1024 * 1024)} MB / {args.TotalBytesToReceive / (1024 * 1024)} MB";
        }

        void IDownloadProgress.SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
        {

        }

        void IDownloadProgress.Show()
        {
            Show();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
