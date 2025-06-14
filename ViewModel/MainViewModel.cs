using Capsa_Connector.Controller.Exceptions;
using Capsa_Connector.Core;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using Capsa_Connector.View;
using NetSparkleUpdater;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.PointOfService;
using Capsa_Connector.Controller.Core.Helpers;
using Capsa_Connector.Core.FileControlParts;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        public DashboardViewModel dashboardVM { get; set; }
        public SettingsViewModel settingsVM { get; set; }
        public DeveloperViewModel developerVM { get; set; }

        public RelayCommand DashboardViewCommand { get; set; }
        public RelayCommand DeveloperViewCommand { get; set; }
        public RelayCommand SettingsViewCommand { get; set; }

        public List<DashboardElementStructure> _previousFileHistory;
        public List<DashboardElementStructure> _activeFileHistory;

        public RelayCommand removeHistory { get; set; }
        public RelayCommand manualUpdate { get; set; }
        public RelayCommand switchThemeMode { get; set; }
        
        public RelayCommand updateGuiRelayCommand { get; set; } 
        
        public RelayCommand UpdateWorkspacesCommand { get; set; }
        public Visibility ExitInAppDialogVisibility { get; set; }
        public int BlurRadius { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }
        private bool _isDeveloper;
        public bool isDeveloper
        {
            get { return _isDeveloper; }
            set
            {
                _isDeveloper = value;
                OnPropertyChanged();
            }
        }

        private string appVersion;
        public string AppVersion
        {
            get
            {
                if (Config.showVersionOnGUI)
                {
                    return $"Ver. {appVersion}";
                }
                else
                {
                    return "";
                }
            }
            set
            {
                appVersion = value;
                OnPropertyChanged();
            }
        }

        public StackPanel TabButtons;
        public Canvas BottomTabButtons;
        public System.Windows.Controls.RadioButton[] radioButtons;
        public System.Windows.Controls.RadioButton activeButton;
        private DiskHandler diskHandler;

        public MainViewModel(System.Windows.Controls.RadioButton[] rbButtons, App app, SparkleUpdater sparkle)
        {
            //DriveInfo[] allDrives = DriveInfo.GetDrives();
            
            string pairingHash = Settings1.Default.pairingAccessToken;
            string appToken = Settings1.Default.appToken;
            
            diskHandler = new DiskHandler();

            appVersion = StaticVariables.appVersion ?? "no version";

            isDeveloper = Settings1.Default.developerMode;
            //diskMappingHandler = new DriveMapper(this);

            radioButtons = rbButtons;

            ExitInAppDialogVisibility = Visibility.Collapsed;
            BlurRadius = 0;
            
            removeHistory = new RelayCommand(o =>
            {
                LocalDataHandling.removeFileHistory();
                _previousFileHistory = new List<DashboardElementStructure>();
                UpdateDashboard();
                Notifications.ShowNotification(
                      "Nastavení",
                      "Historie otevřených souborů byla smazána",
                      Notifications.NotifyTypes.Info,
                      () => { });
            });

            manualUpdate = new RelayCommand(o =>
            {
                sparkle.CheckForUpdatesAtUserRequest();
            });

            dashboardVM = new DashboardViewModel();
            settingsVM = new SettingsViewModel(app, removeHistory, manualUpdate, this);
            developerVM = new DeveloperViewModel();
            
            updateGuiRelayCommand = new RelayCommand(o =>
            {
                dashboardVM.OnViewModelSet();
            });
            
            diskHandler.SetUpdateGuiRelayCommand(updateGuiRelayCommand);

            // load file history
            try
            {
                List<DashboardElementStructure>? fileHistory = LocalDataHandling.getFileHistory();
                _previousFileHistory = fileHistory;
            }
            catch (ReadingLocalEditHistoryCapsaException ex)
            {
                notify(new NotificationStructure { Title = "Načtení předchozích souborů", Text = "Nepodařilo se načíst předchozí uložené soubory", NotifyType = NotifyTypes.Warning });
                _previousFileHistory = new List<DashboardElementStructure>();
            }
            
            dashboardVM.SetPreviousFiles(_previousFileHistory);
            dashboardVM.SetWorkspaces(new List<WorkspaceDashboardStructure>());
            
            
            _activeFileHistory = new List<DashboardElementStructure>();


            CurrentView = dashboardVM;
            activeButton = radioButtons[0];

            DashboardViewCommand = new RelayCommand(async o =>
            {
                // set correct tab
                setCorrectTabSelected(radioButtons[0]);
                CurrentView = dashboardVM;
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                setCorrectTabSelected(radioButtons[1]);
                CurrentView = settingsVM;
            });

            switchThemeMode = new RelayCommand(o =>
            {
                setTabButtonsUnchecked();
                setCorrectTabSelected(activeButton);
                settingsVM.switchTheme();
            });

            DeveloperViewCommand = new RelayCommand(o =>
            {
                setCorrectTabSelected(radioButtons[2]);
                CurrentView = developerVM;
            });
            
            UpdateWorkspacesCommand = new RelayCommand(o =>
            {
                AvailableWorkspacesUpdate();
            });
            dashboardVM.UpdateDashboardCommand = UpdateWorkspacesCommand;

            StaticVariables.taskThreadCommands.CollectionChanged += TaskThreadCommands_CollectionChanged;
        }
        private void TaskThreadCommands_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int countOfEements = StaticVariables.taskThreadCommands.Count();
            log(countOfEements.ToString());
            developerVM._activeThreads.Clear();

            List<ActiveListElement> list = new List<ActiveListElement>();

            foreach (TaskThreadCommandsStructure taskThread in StaticVariables.taskThreadCommands)
            {
                RelayCommand taskThreadKillCommand = new RelayCommand((o) =>
                {
                    taskThread.cancellationTokenSource.Cancel();
                    StaticVariables.taskThreadCommands.Remove(taskThread);
                });

                ActiveListElement activeListElement = new ActiveListElement
                {
                    ElementName = taskThread.name,
                    ElementCommand = taskThreadKillCommand,
                };

                list.Add(activeListElement);
            }

            developerVM.SetActiveThreads(list);
        }

        public void setTabButtonsUnchecked()
        {
            foreach (System.Windows.Controls.RadioButton button in radioButtons)
            {
                button.IsChecked = false;
            }
        }

        public void setCorrectTabSelected(System.Windows.Controls.RadioButton btn)
        {
            activeButton = btn;
            setTabButtonsUnchecked();
            btn.IsChecked = true;
        }

        public void log(string message)
        {
            developerVM.log(message);
        }

        public void addFileToActive(ActiveFile activeFile)
        {
            string fileName = activeFile.fileName;
            string extension = fileName.Split(".")[fileName.Split(".").Count() - 1];

            DashboardElementStructure dashboardElement = new DashboardElementStructure();
            dashboardElement.FileName = fileName;
            dashboardElement.FileExtension = extension;
            dashboardElement.FileURL = activeFile.fileURL;
            dashboardElement.ImageToPreviewImage = ExtIcons.getIconToExtension(extension ?? "");
            dashboardElement.EditTime = DateTime.Now;
            dashboardElement.ActiveFile = activeFile;

            _activeFileHistory.Add(dashboardElement);

            List<DashboardElementStructure> list = _previousFileHistory.ToList();
            list.Add(dashboardElement);

            UpdateDashboard();

            LocalDataHandling.saveFileHistory(list);
        }

        public void removeFileFromActive(ActiveFile activeFile)
        {
            try
            {
                DashboardElementStructure? file = _activeFileHistory.FindAll(x => x.ActiveFile == activeFile).First();
                _previousFileHistory.Add(file);
                _activeFileHistory.Remove(file);
                UpdateDashboard();
            }
            catch (Exception ex)
            {
                v("Unable to remove file from active:");
                v(ex.Message);
            }
        }

        public void UpdateDashboard()
        {

            dashboardVM.SetActiveFiles(_activeFileHistory);
            dashboardVM.SetPreviousFiles(_previousFileHistory);
        }
        
        public void ShowExitInAppDialog()
        {
            BlurRadius = 10;
            ExitInAppDialogVisibility = Visibility.Visible;
        }
        
        public async void AvailableWorkspacesUpdate()
        {
            string appToken = Settings1.Default.appToken;
            if (string.IsNullOrEmpty(appToken)) return;
            v("Updating workspaces");

            string email = Settings1.Default.email;
            if(!string.IsNullOrEmpty(email)) diskHandler.SetEmail(email);

            try
            {
                List<WorkspaceDashboardStructure> workspacesList = await diskHandler.GetAvailableWorkspaces(appToken, UpdateWorkspacesCommand);
                dashboardVM.SetWorkspaces(workspacesList);
            }
            catch (Exception e)
            {
                List<WorkspaceDashboardStructure> emptyList = new List<WorkspaceDashboardStructure>();
                dashboardVM.SetWorkspaces(emptyList);
                v("Error updating workspaces");
            }
        }
    }
}
