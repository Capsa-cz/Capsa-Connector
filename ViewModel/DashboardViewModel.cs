using Capsa_Connector.Controller.Core;
using Capsa_Connector.Core;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.Core.FileControlParts;
using Capsa_Connector.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Capsa_Connector.ViewModel
{
    public class DashboardViewModel : ViewModelBase
    {
        public enum IndicatorState
        {
            Successful = 1,
            Unknown = 2,
            Negative = 3
        }
        public enum IndicatorSpecifier
        {
            Browser = 0,
            Socket = 1,
            LastBrowserActivity = 2
        }

        // default styles definition
        public Style checkmark = (Style)Application.Current.FindResource("Checkmark");
        public Style questionmark = (Style)Application.Current.FindResource("Questionmark");
        public Style minusmark = (Style)Application.Current.FindResource("Minusmark");

        public Style capsaButton = (Style)Application.Current.FindResource("ButtonCapsa");
        public Style circleButton = (Style)Application.Current.FindResource("ButtonCircle");

        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        // checkmark => BrowserPaired from socket / Web accessd at on app token info
        // questionmark => appAccessedAt is null
        private Style _browserPairingStatus;
        public Style BrowserPairingStatus
        {
            get { return _browserPairingStatus; }
            set
            {
                _browserPairingStatus = value;
                OnPropertyChanged(nameof(BrowserPairingStatus));
            }
        }

        // checkmark => active and connected socket thread
        // questionmark => active socket thread but not connected
        // minusmark => app dont have active socket thread
        private Style _socketActiveStatus;
        public Style SocketActiveStatus
        {
            get { return _socketActiveStatus; }
            set
            {
                _socketActiveStatus = value;
                OnPropertyChanged(nameof(SocketActiveStatus));
            }
        }

        // checkmark => active in last 24h
        // questionmark => active in last 7 days
        // minusmark => non active in last 7 days 
        private Style _lastBrowserActivityStatus;
        public Style LastBrowserActivityStatus
        {
            get { return _lastBrowserActivityStatus; }
            set
            {
                _lastBrowserActivityStatus = value;
                OnPropertyChanged(nameof(LastBrowserActivityStatus));
            }
        }

        private Style _pairAgainButtonStyle;
        public Style PairAgainButtonStyle
        {
            get { return _pairAgainButtonStyle; }
            set
            {
                _pairAgainButtonStyle = value;
                OnPropertyChanged(nameof(PairAgainButtonStyle));
            }
        }


        public List<DashboardElementStructure> _activeFiles;
        public ObservableCollection<DashboardElementStructure> ActiveFiles
        {
            get
            {
                ObservableCollection<DashboardElementStructure> observable = new ObservableCollection<DashboardElementStructure>();
                for (int i = _activeFiles.Count() - 1; i >= 0; i--)
                {
                    observable.Add(_activeFiles[i]);
                }
                return observable;
            }
        }
        public void SetActiveFiles(List<DashboardElementStructure> activeFiles)
        {
            _activeFiles = activeFiles;
            OnPropertyChanged(nameof(ActiveFiles));
        }


        public List<DashboardElementStructure> _previousFiles;
        public ObservableCollection<DashboardElementStructure> PreviousFiles
        {
            get
            {
                ObservableCollection<DashboardElementStructure> observable = new ObservableCollection<DashboardElementStructure>();
                for (int i = _previousFiles.Count() - 1; i >= 0; i--)
                {
                    observable.Add(_previousFiles[i]);
                }
                return observable;
            }
        }
        public int PreviousFilesCount => PreviousFiles?.Count ?? 0;

        public void SetPreviousFiles(List<DashboardElementStructure> previousFiles)
        {
            _previousFiles = previousFiles;
            OnPropertyChanged(nameof(PreviousFiles));
            OnPropertyChanged(nameof(PreviousFilesCount));
        }
        
        public List<WorkspaceDashboardStructure> _workspaces;
        public ObservableCollection<WorkspaceDashboardStructure> Workspaces
        {
            get
            {
                ObservableCollection<WorkspaceDashboardStructure> observable = new ObservableCollection<WorkspaceDashboardStructure>();
                for (int i = _workspaces.Count() - 1; i >= 0; i--)
                {
                    observable.Add(_workspaces[i]);
                }
                return observable;
            }
        }
        public int WorkspacesCount => Workspaces?.Count ?? 0;

        public void SetWorkspaces(List<WorkspaceDashboardStructure> workspaces)
        {
            _workspaces = workspaces;
            OnPropertyChanged(nameof(Workspaces));
            OnPropertyChanged(nameof(WorkspacesCount));
        }

        private Visibility _topSectionVisibility;
        public Visibility TopSectionVisibility
        {
            get { return _topSectionVisibility; }
            set
            {
                _topSectionVisibility = value;
                OnPropertyChanged(nameof(TopSectionVisibility));
            }
        }


        public void SetIndicatorState(IndicatorSpecifier indicatorSpecifier, IndicatorState indicatorState)
        {
            Style[] bindingStyles = { BrowserPairingStatus, SocketActiveStatus, LastBrowserActivityStatus };
            Style[] styles = { checkmark, questionmark, minusmark };

            switch ((int)indicatorSpecifier)
            {
                case 0:
                    BrowserPairingStatus = styles[(int)indicatorState - 1];
                    break;
                case 1:
                    SocketActiveStatus = styles[(int)indicatorState - 1];
                    break;
                case 2:
                    LastBrowserActivityStatus = styles[(int)indicatorState - 1];
                    break;
            }
            updatePairAgainButtonStlye();
        }
        public int GetIndicatorState(IndicatorSpecifier indicatorSpecifier)
        {
            Style[] bindingStyles = { BrowserPairingStatus, SocketActiveStatus, LastBrowserActivityStatus };
            Style activeStyle = bindingStyles[(int)indicatorSpecifier];

            if (activeStyle == checkmark) { return 1; }
            else if (activeStyle == questionmark) { return 2; }
            else if (activeStyle == minusmark) { return 3; }
            else throw new Exception("GetIndicatorState -> Wrong indicatorSpecifier");
        }

        public RelayCommand RemoveAppToken { get; set; }
        public RelayCommand PairAgain { get; set; }
        public RelayCommand UpdateDashboardIndicator { get; set; }
        
        public RelayCommand? UpdateDashboardCommand { get; set; }

        public DashboardViewModel()
        {
            _activeFiles = new List<DashboardElementStructure>();
            _previousFiles = new List<DashboardElementStructure>();
            _email = "";
            _fullName = "";
            _browserPairingStatus = questionmark;
            _socketActiveStatus = questionmark;
            _lastBrowserActivityStatus = questionmark;

            Email = Settings1.Default.email;
            FullName = $"{Settings1.Default.firstName} {Settings1.Default.lastName}";

            // Tom section visibility on start
            if (string.IsNullOrEmpty(Email)) { _topSectionVisibility = Visibility.Collapsed; }
            else { _topSectionVisibility = Visibility.Visible; }

            RemoveAppToken = new RelayCommand(o =>
            {
                Settings1.Default.appToken = null;
                Settings1.Default.email = null;
                Settings1.Default.firstName = null;
                Settings1.Default.lastName = null;
                Settings1.Default.pairingAccessToken = null;
                Settings1.Default.Save();

                // end socket threads (they have app token in cookie)
                foreach (TaskThreadCommandsStructure cancellationToken in StaticVariables.taskThreadCommands)
                {
                    cancellationToken.cancellationTokenSource.Cancel();
                }
                StaticVariables.taskThreadCommands.Clear();

                Email = string.Empty;
                FullName = string.Empty;

                TopSectionVisibility = Visibility.Collapsed;
                UpdateDashboard();
            });

            PairAgain = new RelayCommand(o =>
            {
                SetIndicatorState(IndicatorSpecifier.Browser, IndicatorState.Unknown);
                SetIndicatorState(IndicatorSpecifier.Socket, IndicatorState.Unknown);
                SetIndicatorState(IndicatorSpecifier.LastBrowserActivity, IndicatorState.Unknown);

                // remove appToken a pairing access token
                Settings1.Default.appToken = null;
                Settings1.Default.pairingAccessToken = null;

                // end all threads
                foreach (TaskThreadCommandsStructure cancellationToken in StaticVariables.taskThreadCommands)
                {
                    cancellationToken.cancellationTokenSource.Cancel();
                }
                StaticVariables.taskThreadCommands.Clear();

                // Znovu spustit soket, který bude mít nový app token
                new ProgramControl().StartProgram(forceNewAppToken: true, StaticVariables.mainViewModel);


                //Notifications.ShowNotification("Hash opening", "Hash is not set. Contact our support!", Notifications.NotifyTypes.Warning);
            });

            UpdateDashboardIndicator = new RelayCommand((o) =>
            {
                UpdateDashboard();
            });

            SetIndicatorState(IndicatorSpecifier.Browser, IndicatorState.Unknown);
            SetIndicatorState(IndicatorSpecifier.Socket, IndicatorState.Unknown);
            SetIndicatorState(IndicatorSpecifier.LastBrowserActivity, IndicatorState.Unknown);
            _pairAgainButtonStyle = circleButton;

        }

        public override void OnViewModelSet()
        {
            base.OnViewModelSet();
            UpdateDashboard();
        }
        private async void UpdateDashboard()
        {
            // Call update workspaces command
            if(UpdateDashboardCommand != null) UpdateDashboardCommand.Execute(null);
            // Check if app have appToken
            string appToken = Settings1.Default.appToken;
            if (string.IsNullOrEmpty(appToken))
            {
                SetIndicatorState(IndicatorSpecifier.Browser, IndicatorState.Negative);
                SetIndicatorState(IndicatorSpecifier.Socket, IndicatorState.Negative);
                SetIndicatorState(IndicatorSpecifier.LastBrowserActivity, IndicatorState.Negative);
                PairAgainButtonStyle = capsaButton;
                return;
            }

            // Validate app token
            AppTokenInfo? appTokenInfo = await Task.Run(() => AppToken.GetAppTokenInfo(appToken));
            if (appTokenInfo == null || appTokenInfo.token != appToken) return;

            // Set all appTokenInfo to dashboard
            string? firstName = appTokenInfo?.user?.firstName;
            string? lastName = appTokenInfo?.user?.lastName;
            string? email = appTokenInfo?.user?.email;

            FullName = $"{firstName} {lastName}";
            Email = $"{email}";

            Settings1.Default.firstName = firstName;
            Settings1.Default.lastName = lastName;
            Settings1.Default.email = email;
            Settings1.Default.Save();

            // Top section visibility
            if (!string.IsNullOrEmpty(Email) || !string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            {
                TopSectionVisibility = Visibility.Visible;
            }
            else TopSectionVisibility = Visibility.Collapsed;

            // Web paired ( App token paire indicator)
            if (Settings1.Default.appToken == Settings1.Default.paireAppToken)
            {
                SetIndicatorState(IndicatorSpecifier.Browser, IndicatorState.Successful);
            }
            else SetIndicatorState(IndicatorSpecifier.Browser, IndicatorState.Unknown);

            if (appTokenInfo?.webAccessedAt != null)
            // Last browser activity status
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan timeDifference = currentTime - appTokenInfo.webAccessedAt.Value;
                SetIndicatorState(IndicatorSpecifier.Browser, IndicatorState.Successful);

                if (timeDifference.TotalHours <= 24)
                {
                    SetIndicatorState(IndicatorSpecifier.LastBrowserActivity, IndicatorState.Successful);
                }
                else if (timeDifference.TotalDays <= 7)
                {
                    SetIndicatorState(IndicatorSpecifier.LastBrowserActivity, IndicatorState.Unknown);
                }
                else
                {
                    SetIndicatorState(IndicatorSpecifier.LastBrowserActivity, IndicatorState.Negative);
                }
            }
            updatePairAgainButtonStlye();

        }
        private void updatePairAgainButtonStlye()
        {

            // Pair again button style setting -> all need to be green
            if (GetIndicatorState(IndicatorSpecifier.Browser) == 1 &&
                GetIndicatorState(IndicatorSpecifier.Socket) == 1 &&
                GetIndicatorState(IndicatorSpecifier.LastBrowserActivity) == 1)
            {
                PairAgainButtonStyle = circleButton;
            }
            else PairAgainButtonStyle = capsaButton;
        }
    }
}