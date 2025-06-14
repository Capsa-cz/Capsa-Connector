using System;
using Capsa_Connector.Controller.Core;
using Capsa_Connector.Controller.Tools;
using Capsa_Connector.Core;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Capsa_Connector.Controller.Core.Helpers;
using RestSharp;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.ViewModel
{
    class DeveloperViewModel : ViewModelBase
    {
        public RelayCommand ClearConsole { get; set; }

        public RelayCommand TestNotification { get; set; }
        public RelayCommand TestVPN { get; set; }
        public RelayCommand TestShutdown { get; set; }
        public RelayCommand TestShowActiveFiles { get; set; }
        
        public RelayCommand TestReport { get; set; }
        
        public RelayCommand TestDiskConnection { get; set; }
        
        public RelayCommand TestDiskDisconnection { get; set; }
        
        public DiskHandler DiskHandler { get; set; }

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

        private string capsaUrlText;
        public string CapsaUrlText
        {
            get { return capsaUrlText; }
            set
            {
                capsaUrlText = value;
                OnPropertyChanged(nameof(CapsaUrlText));
            }
        }

        public List<ActiveListElement> _activeThreads;
        public ObservableCollection<ActiveListElement> ActiveThreads
        {
            get
            {
                ObservableCollection<ActiveListElement> observable = new ObservableCollection<ActiveListElement>();
                for (int i = _activeThreads.Count() - 1; i >= 0; i--)
                {
                    observable.Add(_activeThreads[i]);
                }
                return observable;
            }
        }
        public RelayCommand SetCapsaUrl { get; set; }

        public void SetActiveThreads(List<ActiveListElement> activeThreads)
        {
            _activeThreads = activeThreads;
            OnPropertyChanged(nameof(ActiveThreads));
        }



        public void log(string line)
        {
            // get additional info to log 
            ConsoleText = ConsoleText + line;
        }

        public void clearConsole()
        {
            ConsoleText = "";
        }

        public DeveloperViewModel()
        {
            _activeThreads = new List<ActiveListElement>();
            consoleText = "";
            ClearConsole = new RelayCommand((o) =>
            {
                ConsoleText = "";
            });
            SetCapsaUrl = new RelayCommand((o) =>
            {
                Settings1.Default.capsaUrl = capsaUrlText;
                Settings1.Default.Save();

                if (!string.IsNullOrEmpty(capsaUrlText)) StaticVariables.capsaUrl = capsaUrlText;
                else { StaticVariables.capsaUrl = Config.capsaUrlDefault; }

                // restart socket and all process
                new ProgramControl().StartProgram(forceNewAppToken: true);

            });

            TestNotification = new RelayCommand((o) =>
            {
                notify(SpecificNotifications.notificationTest);
            });
            
            TestVPN = new RelayCommand((o) =>
            {
                VPNConnector.TestVPN();
            });
            
            TestShowActiveFiles = new RelayCommand((o) =>
            {
                log("Active files: \n");
                foreach (ActiveFile file in StaticVariables.activeFiles)
                {
                    log(file.fileName + "\n");
                }
            });

            TestReport = new RelayCommand(o =>
            {
                 ErrorReport.report("Test info", ErrorReport.Level.Info);
            });
            

            // TestDiskDisconnection = new RelayCommand((o) =>
            // {
            //     try
            //     {
            //         diskConnection.DisconnectDisk();
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //         throw;
            //     }
            // });
            
            // TestDiskConnection = new RelayCommand((o) =>
            // {
            //     try
            //     {
            //         diskConnection.ConnectToDisk();
            //     }
            //     catch (DiskConnectionException e)
            //     {
            //         log(e.Message);
            //     }
            //     catch (DiskAlreadyConnectedException e)
            //     {   
            //         log(e.Message);
            //     }
            // });
            capsaUrlText = Settings1.Default.capsaUrl;

        }
        
    }
}
