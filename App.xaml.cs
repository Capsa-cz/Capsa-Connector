using Capsa_Connector.Controller.Tools;
using Capsa_Connector.Core;
using Capsa_Connector.Core.Tools;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.WPF;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Capsa_Connector.Controller.Core.Helpers;
using Capsa_Connector.Controller.Tools.CapsaNetSparkle;
using Capsa_Connector.Core.FileControl;
using Capsa_Connector.Model;
using Capsa_Connector.ViewModel;
using NetSparkleUpdater.Interfaces;
using static Capsa_Connector.Core.Tools.Log;
using Forms = System.Windows.Forms;


namespace Capsa_Connector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5; // Show the window normally


        private Forms.NotifyIcon _notifyIcon;
        private Mutex _singleInstanceMutex;
        private MainWindow MainWindow { get; set; }

        public ResourceDictionary ThemeDictionary
        {
            get { return Resources.MergedDictionaries[0]; }
        }

        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // // For between versions settings update
            if (Settings1.Default.UpdateSettings)
            {
                Settings1.Default.Upgrade();
                Settings1.Default.UpdateSettings = false;
                Settings1.Default.Save();
            }
            
            // Logs all startup data
            Log.DumpLogFilesIfLarge();
            StartupLog();

            bool backgroundRun = Array.Exists(e.Args, arg => arg == "background_window");

            // MUTEX HANDLING
            bool createdNew;
            _singleInstanceMutex = new Mutex(true, "CapsaConnectorMutex", out createdNew);

            if (!createdNew)
            {
                // Another instance is already running, so exit
                //MessageBox.Show("The application is already running.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ActivateExistingInstance();
                Current.Shutdown();
                return;
            }
            
            // Set version of app
            SetupVersion();

            SparkleUpdater _sparkle = SetupSparkle();

            MainWindow mainWindow = new MainWindow((App)Application.Current, _sparkle);
            this.MainWindow = mainWindow;


            if (Settings1.Default.autoUpdate)
            {
                _sparkle.StartLoop(true, true);
            }


            //_sparkle.CheckForUpdatesAtUserRequest(true);

            Capsa_Connector.Core.Tools.Log log = new Log();

            if (!backgroundRun)
            {
                mainWindow.Show();
            }

            //#if DEBUG
            //            // Always show app when debug
            //            mainWindow.Show();
            //#endif


            if (Settings1.Default.darkMode)
            {
                var app = (App)Application.Current;
                app.ChangeTheme(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Themes/UIColors-Dark.xaml"));
            }

            // Create icon in system tray
            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon("Resources/favicon.ico"),
                Visible = true
            };
            
            // Create contex menu for icon in system tray
            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add("Okno aplikace", null, AppWindowClick);
            contextMenu.Items.Add("Verze ke stažení", null, VersionsClick);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add("Ukončit", null, Exit_Click);
            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
            
            DiskHandler.ReconnectUnavailableDisks();

            //var app = (App)Application.Current;
            //app.ChangeTheme(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Themes/UIColors-Dark.xaml"));
        }

        private static void StartupLog()
        {
            // Write to console whole startup configuration in Settings1.Default
            Capsa_Connector.Core.Tools.Log.v("Startup configuration:");
            //  Log these in Settings1.Default - dont do serialization (appToken, darkMode, developerMode, notificationsEnabled, autoUpdate,pairingAccessToken, firstName, lastName, email, capsaUrl, paireAppToken, firstStart)
            Capsa_Connector.Core.Tools.Log.v("appToken: " + Settings1.Default.appToken);
            Capsa_Connector.Core.Tools.Log.v("darkMode: " + Settings1.Default.darkMode);
            Capsa_Connector.Core.Tools.Log.v("developerMode: " + Settings1.Default.developerMode);
            Capsa_Connector.Core.Tools.Log.v("notificationsEnabled: " + Settings1.Default.notificationsEnabled);
            Capsa_Connector.Core.Tools.Log.v("autoUpdate: " + Settings1.Default.autoUpdate);
            Capsa_Connector.Core.Tools.Log.v("pairingAccessToken: " + Settings1.Default.pairingAccessToken);
            Capsa_Connector.Core.Tools.Log.v("firstName: " + Settings1.Default.firstName);
            Capsa_Connector.Core.Tools.Log.v("lastName: " + Settings1.Default.lastName);
            Capsa_Connector.Core.Tools.Log.v("email: " + Settings1.Default.email);
            Capsa_Connector.Core.Tools.Log.v("capsaUrl: " + Settings1.Default.capsaUrl);
            Capsa_Connector.Core.Tools.Log.v("paireAppToken: " + Settings1.Default.paireAppToken);
            Capsa_Connector.Core.Tools.Log.v("firstStart: " + Settings1.Default.firstStart);
            // Startup time
            Capsa_Connector.Core.Tools.Log.v("Startup time: " + DateTime.Now.ToString());
        }

        private static SparkleUpdater SetupSparkle()
        {
            v(Config.fullAppcastUrl);
            ILogger sparkleLogger = new CustomLogger();

            string publicKey = "MIsCSod6UbGvVYes/zd6DLxfa3V1Pb7FVLIyIlOljAs=";
            SparkleUpdater _sparkle = new SparkleUpdater(
                Config.fullAppcastUrl,
                new Ed25519Checker(SecurityMode.Unsafe, publicKey) // your base 64 public key -- generate this with the NetSparkleUpdater.Tools.AppCastGenerator .NET CLI tool on any OS
            )
            {
                UIFactory = new CapsaUIFactory
                {
                    HideReleaseNotes = true,
                    HideRemindMeLaterButton = true,
                    HideSkipButton = true,
                }, // or null or choose some other UI factory or build your own!
                RelaunchAfterUpdate = false, // default is false; set to true if you want your app to restart after updating (keep as false if your installer will start your app for you)
                CustomInstallerArguments = "", // set if you want your installer to get some command-line args
            };
            
            _sparkle.LogWriter = sparkleLogger;
            return _sparkle;
        }

        private static void SetupVersion()
        {
            // Getting version
            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            if (fileVersionInfo.FileVersion == null)
                throw new Exception("Cannot get app version");
            // Setting version
            StaticVariables.appVersion = fileVersionInfo.FileVersion;
        }

        private void ActivateExistingInstance()
        {
            using (var currentProcess = Process.GetCurrentProcess())
            {
                Trace.WriteLine(currentProcess.ProcessName);
                foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    SetForegroundWindow(process.MainWindowHandle);
                }
            }
        }

        private void VersionsClick(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(Config.capsaUrlForDownloadingApp) { UseShellExecute = true });
        }

        private void AppWindowClick(object? sender, EventArgs e)
        {
            MainWindow.Show();
            if (Current.MainWindow != null) Current.MainWindow.Activate();
        }
        
        private void NotifyIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                MainWindow.Show();
                if (Current.MainWindow != null) Current.MainWindow.Activate();
            }
        }

        private ShutdownMode _shutdownMode = ShutdownMode.OnExplicitShutdown;
        protected override void OnExit(ExitEventArgs e)
        {
            
            
            try
            {
                // Stop all connections
                MainWindow.OpenExitInAppDialog();
            
                EndConnections.EndAllFileConnections();
                EndConnections.EndAllThreads();
                
                // Remove icon in system tray
                _notifyIcon.Dispose();

                // Release mutex
                _singleInstanceMutex.ReleaseMutex();
                _singleInstanceMutex.Dispose();
                base.OnExit(e);
            }
            catch (Exception ex)
            {
                v("OnExit -> " + ex.Message);
            }
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            e.Cancel = true;
            MainWindow.OpenExitInAppDialog();

            //this.Shutdown();
            //base.OnSessionEnding(e);        
        }

       

        private void Exit_Click(object sender, EventArgs e)
        {
            this.MainWindow.OpenExitInAppDialog();
        }
    }
}