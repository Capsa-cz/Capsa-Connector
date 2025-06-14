using Capsa_Connector.ViewModel;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF;
using NetSparkleUpdater.UI.WPF.Controls;
using NetSparkleUpdater.UI.WPF.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Capsa_Connector.View
{
    /// <summary>
    /// Interaction logic for AvailableUpdateWindow.xaml
    /// </summary>
    public partial class AvailableUpdateWindow : Window, IUpdateAvailable
    {
        private readonly AvailableUpdateViewModel _viewModel;
        public AvailableUpdateWindow(AvailableUpdateViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            // Bind UI elements to ViewModel properties and commands


            //BtnSkip.Click += (sender, e) => _viewModel.OnSkipClicked();
            BtnUpdate.Click += (sender, e) => UserResponded?.Invoke(this, new UpdateResponseEventArgs(UpdateAvailableResult.InstallUpdate, CurrentItem));
            //BtnRemind.Click += (sender, e) => _viewModel.OnRemindMeLaterClicked();

            TbVersion.Text = CurrentItem.Version;
            string currentAppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            TbActualVersion.Text = "Aktuální: " + currentAppVersion;
            
            DataContext = _viewModel;
        }

        private void OnUserResponded(object sender, UpdateAvailableResult result)
        {
            Trace.WriteLine("User response");
            // Handle user response (e.g., close window, initiate download)
            Close();
        }

        #region Implementation of IUpdateAvailable

        public UpdateAvailableResult Result => _viewModel.Result;
        public AppCastItem CurrentItem => _viewModel.CurrentItem;

        public event UserRespondedToUpdate UserResponded;

        public void Show(bool IsOnMainThread)
        {
            if (IsOnMainThread)
            {
                Dispatcher.Invoke(() => Show());
                BringToFront();
            }
            else
            {
                Show();
                BringToFront();
            }
        }

        public void HideReleaseNotes()
        {
            //_viewModel.HideReleaseNotes();
        }

        public void HideRemindMeLaterButton()
        {
            //_viewModel.HideRemindMeLaterButton();
            //BtnRemind.Visibility = Visibility.Collapsed;
        }

        public void HideSkipButton()
        {
            //_viewModel.HideSkipButton();
            //BtnSkip.Visibility = Visibility.Collapsed;
        }

        public void BringToFront()
        {
            Activate();
            Focus();
        }

        #endregion
    }
}
