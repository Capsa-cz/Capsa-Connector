using Capsa_Connector.Controller.Core;
using Capsa_Connector.Core;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.Core.FileControlParts;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using Capsa_Connector.View;
using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Capsa_Connector.Controller.Tools;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private App _app;
        private ViewModelBase _mainViewModel;
        private bool isDeveloper;
        private bool isDakmode;
        private bool isNotificationEnabled;
        private bool isAutoupdateEnabled;
        private bool isWorkspaceRenaming;

        public bool IsDeveloper
        {
            get { return isDeveloper; }
            set
            {
                ((MainViewModel)_mainViewModel).isDeveloper = value;
                isDeveloper = value;
                Settings1.Default.developerMode = value;
                Settings1.Default.Save();
                OnPropertyChanged();
            }
        }
        public RelayCommand removeHistory { get; set; }
        public RelayCommand manualUpdate { get; set; }
        public RelayCommand openHash { get; set; }
        public RelayCommand getNewAppToken { get; set; }
        public RelayCommand StartClientTask { get; set; }
        public RelayCommand killThreads { get; set; }
        public RelayCommand testNewWindow { get; set; }
        public RelayCommand changeTheme { get; set; }
        public bool IsDakmode
        {
            get { return isDakmode; }
            set
            {
                isDakmode = value;
                Settings1.Default.darkMode = value;
                if (value)
                {
                    //_app.ChangeTheme(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Themes/UIColors-Dark.xaml"));
                    App.Current.Resources.MergedDictionaries[0].Clear();
                    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri(AppDomain.CurrentDomain.BaseDirectory + "Themes/UIColors-Dark.xaml") });
                }
                else
                {
                    //_app.ChangeTheme(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Themes/UIColors-Light.xaml"));
                    App.Current.Resources.MergedDictionaries[0].Clear();
                    App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new System.Uri(AppDomain.CurrentDomain.BaseDirectory + "Themes/UIColors-Light.xaml") });
                }
                Settings1.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool IsNotificationsEnabled
        {
            get { return isNotificationEnabled; }
            set
            {
                isNotificationEnabled = value;
                Settings1.Default.notificationsEnabled = value;
                Settings1.Default.Save();
            }
        }

        public bool IsAutoupdateEnabled
        {
            get { return isAutoupdateEnabled; }
            set
            {
                isAutoupdateEnabled = value;
                Settings1.Default.autoUpdate = value;
                Settings1.Default.Save();
            }
        }
        
        public bool IsWorkspaceRenaming
        {
            get { return isWorkspaceRenaming; }
            set
            {
                isWorkspaceRenaming = value;
                Settings1.Default.DiskRenaming = value;
                Settings1.Default.Save();
            }
        }

        public void switchTheme()
        {
            IsDakmode = !IsDakmode;
        }

        public SettingsViewModel(App app, ICommand removeHistoryApp, RelayCommand manualUpdateApp, ViewModelBase mainViewModel)
        {
            isDeveloper = Settings1.Default.developerMode;
            IsDakmode = Settings1.Default.darkMode;
            isNotificationEnabled = Settings1.Default.notificationsEnabled;
            isAutoupdateEnabled = Settings1.Default.autoUpdate;
            _app = app;
            _mainViewModel = mainViewModel;

            changeTheme = new RelayCommand(o =>
            {
                switchTheme();
            });

            removeHistory = new RelayCommand(o =>
            {
                removeHistoryApp.Execute(null);
            });

            manualUpdate = new RelayCommand(o =>
            {
                manualUpdateApp.Execute(null);
            });

            openHash = new RelayCommand(o =>
            {
                string hash = Settings1.Default.pairingAccessToken;
                Trace.WriteLine("hash is :" + hash);
                if (hash != "" && hash != null)
                {
                    var psi = new ProcessStartInfo
                    {
                        //Hash vykopat do nastavení, ukládat, využívat hojně 
                        FileName = Config.http + StaticVariables.capsaUrl + "/at/" + hash,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                    return;
                }
                Notifications.ShowNotification("Hash opening", "Hash is not set. Contact our support!", Notifications.NotifyTypes.Warning);
            });

            getNewAppToken = new RelayCommand(async o =>
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                StaticVariables.taskThreadCommands.Add(new Model.TaskThreadCommandsStructure("GetAppTokenTashThread", cts));

                string? appToken = Task.Run(() => AppToken.GetAppToken(cancellationToken: token, saveToSettings: true)).Result.appToken;

                Settings1.Default.email = null;
                Settings1.Default.firstName = null;
                Settings1.Default.lastName = null;
                Settings1.Default.Save();
            });

            StartClientTask = new RelayCommand(async o =>
            {
                StartNewClient();
            });

            killThreads = new RelayCommand(async o =>
            {
                foreach (TaskThreadCommandsStructure cancellationToken in StaticVariables.taskThreadCommands)
                {
                    Trace.WriteLine("§ ENDING THREAD §");
                    cancellationToken.cancellationTokenSource.Cancel();
                }
                StaticVariables.taskThreadCommands.Clear();
            });
            
            
        }
        private async void StartNewClient()
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken token = cts.Token;
                StaticVariables.taskThreadCommands.Add(new Model.TaskThreadCommandsStructure("StartClientTaskThread", cts));

                // this will run program on separated thread and main thread is free
                Task.Run(() => ProgramControl.StartClientTask(token, (MainViewModel)_mainViewModel));
            }
            catch (System.Exception e)
            {
                v(e.Message, true);
                if (e.StackTrace != null) v(e.StackTrace);
                return;
            }
        }
    }
}