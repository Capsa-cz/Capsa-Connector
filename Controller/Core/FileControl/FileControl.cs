using Capsa_Connector.Controller.Exceptions;
using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Model;
using Capsa_Connector.View;
using Capsa_Connector.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Capsa_Connector.Core.Bases;
using static Capsa_Connector.Core.StaticVariables;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Windows.Forms.Timer;

namespace Capsa_Connector.Core.FileControl
{
    /// <summary>
    /// <c>FileControl</c> is responsible for opening and watching file
    /// </summary>
    /// 
    partial class FileControl
    {
        // For setting process to foreground
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, uint nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern bool BringWindowToTop(IntPtr handle);

        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool LockSetForegroundWindow(uint uLockCode);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool AllowSetForegroundWindow(uint dwProcessId);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern IntPtr SetFocus(IntPtr handle);
        [DllImport("User32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        const int SW_RESTORE = 9;

        // Static
        private MainViewModel mainViewModel;
        private WebSocketClientHandler webSocketClientHandler;
        private string fileAccessToken;
        private string fileUrl;

        // Runtime variables
        private ActiveFile? activeFile;
        private string? fileName;
        private string? directory;

        /// <summary>
        /// Prepare file control variables
        /// </summary>
        /// <param name="_webSocketClientHandler">Actual socket client handler</param>
        /// <param name="_mainViewModel">Main View Model to show on dashboard</param>
        /// <param name="_fileAccessToken">Accessing file token from server</param>
        /// <param name="_fileUrl">File urll from server</param>
        public FileControl(WebSocketClientHandler _webSocketClientHandler, MainViewModel _mainViewModel, string _fileAccessToken, string _fileUrl)
        {
            mainViewModel = _mainViewModel;
            webSocketClientHandler = _webSocketClientHandler;
            fileAccessToken = _fileAccessToken;
            fileUrl = _fileUrl;
        }

        /// <summary>
        /// Extend lock, download file and start online edit and watcher
        /// </summary>
        public void StartOnlineEdit()
        {
            // Firstly it will extend lock of file before any action
            activeFile = new ActiveFile(fileAccessToken, fileUrl);
            OpenStartEditWindow(activeFile.fileName);
            
            lock(StaticVariables.activeFiles)
            {
                StaticVariables.activeFiles.Add(activeFile);
            }

            // Download file
            try
            {
                string filePath = Task.Run(async () => await DownloadFile(fileAccessToken)).Result;
                fileName = Path.GetFileName(filePath);
                directory = Path.GetDirectoryName(filePath);
                activeFile.fileName = fileName;
                activeFile.filePath = filePath;
                if (StaticVariables.startEditWindow != null) StaticVariables.startEditWindow.FileName = fileName;

                if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(filePath)) throw new Exception("Chyba při stahování");
                if (activeFile == null) throw new Exception("Chyba při stahování");
            }
            catch (Exception ex)
            {
                notify(new NotificationStructure { Title = "Chyba stahování", Text = "Při stahování souboru došlo k chybě" });
                v(ex.Message);
                v(ex.StackTrace);
                StaticVariables.startEditWindow?.Close();

                // Unlock online file to not be locked unnecessarily -> no need for it. It is not locked
                //Task.Run(async () => await CancelAndUnlockFile(fileAccessToken));
                return;
            }

            // Open file and start watching them
            try
            {
                Task.Run(async () => { await OpenAndWatchFileAsync(activeFile); });
            }
            catch
            {
                StaticVariables.activeFiles.TryTake(out ActiveFile? removedFile);
            }
        }

        private void OpenStartEditWindow(string fileName)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (StaticVariables.startEditWindow == null)
                {
                    StaticVariables.startEditWindow = new StartEditWindow("");
                    StaticVariables.startEditWindow.Show();
                }
                else
                {
                    StaticVariables.startEditWindow.FileName = fileName;
                    StaticVariables.startEditWindow.Show();
                }

                StaticVariables.startEditWindow.Focus();
                StaticVariables.startEditWindow.Activate();
                StaticVariables.startEditWindow.Topmost = true;
            });
        }
        
        /// <summary>
        /// Watching of active file and calling action methods
        /// </summary>
        /// <returns></returns>
        async Task OpenAndWatchFileAsync(ActiveFile _activeFile)
        {
            try
            {
                OpenFile(_activeFile.filePath!);
                mainViewModel.addFileToActive(_activeFile);
            }
            catch (OpenFileCapsaException e)
            {
                v(e.Message);
                notify(new NotificationStructure { Text = "Nepodařilo se otevřít soubor", Title = "Online editace", NotifyType = NotifyTypes.Error });
                mainViewModel.removeFileFromActive(_activeFile);
                
            }
            CloseStartEditWindow();

            DataCommandContainer commandParams = new DataCommandContainer { fileAccessToken = _activeFile.accessToken };
            await webSocketClientHandler.SendCommand("file-opened", commandParams);
            StaticVariables.pairCommandSent = DateTime.Now;
            _activeFile.fileOpenedSended = true;
            
            
            // Check if file is locked
            bool fileLocked = CheckFileWasLocked(_activeFile.filePath!, 3);
            _activeFile.hasLocking = fileLocked;

            
            // Set watcher to file - no matter if the lock is here
            FileSystemWatcher watcher = SetWatcher();
            _activeFile.watcher = watcher;
            
            // It will also move file to previous edited, so it will not disappear
            if(!fileLocked) mainViewModel.removeFileFromActive(_activeFile);
            
            if (_activeFile.hasLocking) Lock();
        }

        private async void Lock()
        {
            try
            {
                await Task.Run(() => ExtendFileLock(fileAccessToken));
                activeFile!.lockExtended = DateTime.Now;
            }
            catch (ExtendFileLockCapsaException e)
            {
                v(e.Message);
                if (e.httpStatusCode is System.Net.HttpStatusCode.Conflict)
                {
                    notify(new NotificationStructure { Title = "Prodloužení zámku", Text = "Soubor již někdo upravuje!" });
                    return;
                }
                notify(new NotificationStructure { Title = "Prodloužení zámku", Text = "Nepodařilo se prodloužit zámek" });
                return;
            }
        }

        private void CloseStartEditWindow()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (StaticVariables.startEditWindow != null)
                {
                    StaticVariables.startEditWindow.Close();
                    StaticVariables.startEditWindow = null;
                }
            });
        }
        
        private bool IsFileUsedByProcess(string filePath)
        {
            var processes = Win32Processes.GetProcessesLockingFile(filePath);
            if (processes.Count == 0) return false;
            return true;
        }
        


        private async Task SendFileOpened(ActiveFile _activeFile)
        {
            // Send file opened
            try
            {
                DataCommandContainer commandParams = new DataCommandContainer { fileAccessToken = _activeFile.accessToken };
                await webSocketClientHandler.SendCommand("file-opened", commandParams);
                StaticVariables.pairCommandSent = DateTime.Now;
                _activeFile.fileOpenedSended = true;
            }
            catch (Exception e)
            {
                v(e.Message);
            }
        }

        /// <summary>
        /// Open file in default application by file path
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="OpenFileCapsaException">When problem occured on file opening</exception>
        private void OpenFile(string filePath)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.FileName = filePath;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                p.Start();
                try
                {
                    SetWindowToForeground(p);
                }
                catch (Exception e)
                {
                    v("Unable to set to foreground");
                }
            }
            catch (ShowInForegroundCapsaException e)
            {
                v(e.Message);
                v(e.StackTrace);
                notify(new NotificationStructure() { Title = "Otevření na popředí", Text = "Nepodařilo se umístit okno do popředí", NotifyType = NotifyTypes.Warning });
                return;
            }
            catch (Exception e)
            {
                v(e.Message);
                v(e.StackTrace);
                throw new OpenFileCapsaException("Problem occured when opening file", e);
            }
        }

        private static void SetWindowToForeground(Process p)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if(StaticVariables.startEditWindow != null)
                {
                    StaticVariables.startEditWindow.Hide();
                    StaticVariables.startEditWindow.Show();
                    StaticVariables.startEditWindow.Activate();
                    StaticVariables.startEditWindow.Topmost = true;
                    StaticVariables.startEditWindow.Focus();
                }
                // var mainWindow = Application.Current.MainWindow;
                // if (mainWindow == null) throw new ShowInForegroundCapsaException("Main window is null");
                // mainWindow.Hide();
                // mainWindow.Show();
                // mainWindow.WindowState = WindowState.Normal;
                // mainWindow.Activate();
                // mainWindow.Topmost = true;
                // mainWindow.Focus();
                // mainWindow.Activate();
            });

            // If waitForInputIdle is taking more then 2 seconds, it will go to next step
            
            p.WaitForInputIdle(2000);
            
    
            // Wait until the process's main window handle is not null
            for (int i = 0; i < 10 && p.MainWindowHandle == IntPtr.Zero; i++)
            {
                Thread.Sleep(500);
            }
            
            //1) The foreground process has not disabled calls to SetForegroundWindow by a previous call to the LockSetForegroundWindow function.)
            LockSetForegroundWindow(2); // Unlock
            
            const uint SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2000;
            SystemParametersInfo(SPI_SETFOREGROUNDLOCKTIMEOUT, 0, 0, 0);
            
            uint foreThread = 0;
            uint threadIdentify = GetWindowThreadProcessId(p.MainWindowHandle, out foreThread);
            uint appThread = (uint)GetCurrentThreadId();
            
            v($"Foreground thread attaching log -> ThreadIdentify: {threadIdentify}, Fore thread: {foreThread}, App thread: {appThread}", false);
            
            if (foreThread != appThread) {
                AttachThreadInput(foreThread, appThread, true);
                BringWindowToTop(p.MainWindowHandle);
                ShowWindow(p.MainWindowHandle, 5);
                AttachThreadInput(foreThread, appThread, false);
            }
            else
            {
                BringWindowToTop(p.MainWindowHandle);
                ShowWindow(p.MainWindowHandle, 5);
            }

            // Set active window to foreground
            SetForegroundWindow(p.MainWindowHandle);
            
            // Hide main window
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null) throw new ShowInForegroundCapsaException("Main window is null");
                mainWindow.Topmost = false;
                mainWindow.Hide();
            });
            
        }

        private FileSystemWatcher SetWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                NotifyFilter = NotifyFilters.Attributes
                                             | NotifyFilters.FileName
                                             | NotifyFilters.LastAccess
                                             | NotifyFilters.LastWrite
                                             | NotifyFilters.Security
                                             | NotifyFilters.Size
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;
            watcher.Disposed += OnDispose;

            watcher.Path = directory!;
            watcher.Filter = fileName!;
            
            if (activeFile!.hasLocking) Task.Run(CreateLockLoop);

            watcher.EnableRaisingEvents = true;
            
            return watcher;
        }

        /// <summary>
        /// Will check if lock is active, if yes, then after 10 minutes it will extend lock
        /// If no it will end file edit
        /// </summary>
        private async void CreateLockLoop()
        {
            var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                if (IsFileUsedByProcess(activeFile!.filePath!))
                {
                    if(DateTime.Now - activeFile.lockExtended > TimeSpan.FromMinutes(10)) Lock();
                }
                else
                {
                    if(activeFile.uploadActive) continue;
                    EndFileEditAndUnlock();
                    break;
                }
            }
        }

        /// <summary>
        /// Function is called only on locked / files with locking.
        /// </summary>
        private void EndFileEditAndUnlock()
        {
            if(activeFile == null) return;

            int secondsWaited = 0;
            while (activeFile.uploadActive && !activeFile.errorOnUpload && secondsWaited < Config.maxWaitingTimeOfUploadWhenClose)
            {
                Thread.Sleep(1000);
                v("Waiting for upload to end - time waited " + secondsWaited);
                secondsWaited++;
            }
            
            if (activeFile.errorOnUpload)
            {
                notify(new NotificationStructure { Title = "Chyba editace", Text = "Nastala chyba editace", NotifyType = NotifyTypes.Error });
                Task.Run(() => CancelAndUnlockFile(activeFile.accessToken));
            }

            if (!activeFile.wasUpdated)
            {
                Task.Run(() => CancelAndUnlockFile(activeFile.accessToken));
            }

            if (activeFile.wasUpdated && activeFile.uploadSuccessfull) {
                bool confirmed = Task.Run(() => ConfirmFileOnlineEdit(activeFile)).Result;
                if (confirmed) {
                    // Callback is action which will open url of file on web
                    notify(new NotificationStructure
                    {
                        Text = "Váš soubor byl úspěšně uložen",
                        Title = this?.activeFile?.fileName ?? "",
                        NotifyType = NotifyTypes.Info,
                        callback = new Action(() =>
                        {
                            Process.Start(activeFile.fileURL);
                        })
                    });
                }
            }
            
            mainViewModel.removeFileFromActive(activeFile!);
            StaticVariables.activeFiles.TryTake(out ActiveFile? removedFile);
            activeFile!.wasCommitted = true;
            activeFile?.watcher?.Dispose();
            activeFile = null;
        }

        private bool CheckFileWasLocked(string filePath)
        {
            var processes = Win32Processes.GetProcessesLockingFile(filePath);
            if (processes.Count == 0) return false;
            return true;
        }

        private bool CheckFileWasLocked(string filePath, int maxSecounds)
        {
            bool fileLocked = false;
            int repeats = 0;
            while (repeats < maxSecounds && fileLocked != true)
            {
                Thread.Sleep(1000);
                fileLocked = CheckFileWasLocked(filePath);
                repeats++;
            }
            v("Checking file lock: " + fileLocked, true);

            return fileLocked;
        }

        /// <summary>
        /// Responsible for checking of changes and uploading to server if there are any change on file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="Exception"></exception>
        async void OnChanged(object sender, FileSystemEventArgs e)
        {
            // Check for real change
            if (e.ChangeType != WatcherChangeTypes.Changed) return;
            
            ActiveFile? myActiveFile = StaticVariables.activeFiles.FirstOrDefault(activeFile => activeFile.filePath == e.FullPath);
            
            if (myActiveFile != null) { v("FileControl -> Onchange -> activeFilePath: " + myActiveFile.filePath); }

            // Error handling (it cant be null)
            if (myActiveFile == null)
            {
                v($"FileControl -> Onchange -> ActiveFile null");
                return;
            }

            await PrepareAndUploadActiveFile(myActiveFile);
        }

        private async Task PrepareAndUploadActiveFile(ActiveFile activeFile)
        {
            activeFile.uploadActive = true;
            // Directory stuff for MD5
            var directory = Path.GetDirectoryName(activeFile.filePath);
            var fileName = Path.GetFileNameWithoutExtension(activeFile.filePath);
            var extension = Path.GetExtension(activeFile.filePath);
            var copyFilePath = directory + "\\" + fileName + "md5Check" + extension;
            
            // Checking for md5Check filepath if it is empty (only used for calculing md5)
            if (File.Exists(copyFilePath))
            {
                if (IsFileLocked(copyFilePath)) throw new Exception("Copy file is used somewhere and i cant access it");
                else File.Delete(copyFilePath);
            }

            // Dont remove my sleep
            Thread.Sleep(40);

            // Prepare copy of file for creating MD5
            try
            {
                File.Copy(activeFile.filePath, copyFilePath);
            }
            catch (Exception ex)
            {
                Thread.Sleep(20);
                try
                {
                    File.Copy(activeFile.filePath, copyFilePath);
                }
                catch (Exception ex2)
                {
                    v(ex2.Message);
                    activeFile.uploadActive = false;
                    return;
                }
            }

            // Crate new MD5
            string newMD5 = CalculateMD5(copyFilePath);
            v($"FileControl -> Onchange -> MD5 Calculate: {newMD5}");

            // Remove copy used for md5 creating
            File.Delete(copyFilePath);
            v($"Copy file deleted from {copyFilePath}");

            // Check for md5 change and set it if its new
            if (activeFile.md5Hash == newMD5)
            {
                activeFile.uploadActive = false;
                return;
            }
            activeFile.md5Hash = newMD5;

            // Upload file - first try
            bool uploadCompleted = false;
            try
            {
                uploadCompleted = await UploadFile(activeFile);
            }
            catch (Exception ex)
            {
                activeFile.uploadActive = false;
                v(ex.Message);
            }

            if (activeFile.uploadAgaing && uploadCompleted == false)
            {
                uploadCompleted = await UploadFile(activeFile);
            }

            // Log if upload was not successfull
            if (!uploadCompleted)
            { 
                v($"Upload was not ended correctly on file: {fileName}");
                // Open window with option to save or try again upload

                RelayCommand repeatUpload = new RelayCommand( (o) =>
                {
                    try
                    {
                        bool successfull = Task.Run(async () => await UploadFile(activeFile: activeFile)).Result;
                        if (successfull) CloseUploadErrorWindow();
                        else
                            notify(new NotificationStructure
                            {
                                Title = "Chyba nahrání",
                                Text = "Při opětovném nahrání došlo k chybě",
                                NotifyType = NotifyTypes.Error
                            });
                    }
                    catch (Exception e)
                    {
                        v("Retry upload error:" + e.Message);
                    }

                });
                
                RelayCommand saveFile = new RelayCommand((o) =>
                {
                     SaveFileDialog saveFileDialog = new SaveFileDialog();

                    saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    saveFileDialog.Title = "Uložit soubor";
                    saveFileDialog.FileName = this.activeFile.fileName;

                    DialogResult result = saveFileDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string pathToFile = saveFileDialog.FileName;

                        try
                        {
                            // if file exist -> delete it
                            if (File.Exists(pathToFile))
                            {
                                File.Delete(pathToFile);
                            }
                            File.Copy(this.activeFile.filePath, pathToFile);
                            v("File was saved to " + pathToFile);
                            notify(new NotificationStructure
                            {
                                Title = "Soubor uložen",
                                Text = "Soubor byl úspěšně uložen",
                                NotifyType = NotifyTypes.Info
                            });
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                StaticVariables.uploadErrorWindow.Hide();
                                StaticVariables.uploadErrorWindow.Close();
                            });

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Při ukládání souboru došlo k chybě: " + ex.Message);
                        }
                    }
                });
                
                OpenUploadErrorWindow(activeFile, repeatUpload, saveFile);
                
                activeFile.errorOnUpload = true;
                activeFile.uploadActive = false;
            }
        }
        

        private void OpenUploadErrorWindow(ActiveFile activeFile1, RelayCommand repeatUpload, RelayCommand saveFile)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if(StaticVariables.uploadErrorWindow != null)
                {
                    StaticVariables.uploadErrorWindow.Hide();
                    StaticVariables.uploadErrorWindow.Close();
                    StaticVariables.uploadErrorWindow = null;
                }
                StaticVariables.uploadErrorWindow = new UploadErrorWindow(activeFile1, repeatUpload, saveFile);
                StaticVariables.uploadErrorWindow.Show();
                StaticVariables.uploadErrorWindow.Activate();
            });
        }
        private void CloseUploadErrorWindow()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (StaticVariables.uploadErrorWindow != null)
                {
                    StaticVariables.uploadErrorWindow.Hide();
                    StaticVariables.uploadErrorWindow.Close();
                    StaticVariables.uploadErrorWindow = null;
                }
            });
        }

        /// <summary>
        /// Called when file is created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnCreated(object sender, FileSystemEventArgs e)
        {
            v($"Created: {e.FullPath}");
        }
        /// <summary>
        /// Called when file is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDeleted(object sender, FileSystemEventArgs e)
        {

            v($"Deleted: {e.FullPath}");
        }
        /// <summary>
        /// Called when file is renamed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnRenamed(object sender, RenamedEventArgs e)
        {
            /*
            v($"<Renamed:");
            v($"  Old: {e.OldFullPath}");
            v($"  New: {e.FullPath}");
            v("End of rename log>");
            */
        }
        /// <summary>
        /// Called when error ocured with file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnError(object sender, ErrorEventArgs e)
        {
            v(e.GetException().ToString());
        }
        /// <summary>
        /// Called when file stops tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDispose(object? sender, EventArgs e)
        {
            v("OnDispose");
            mainViewModel.removeFileFromActive(activeFile);
        }
        /// <summary>
        /// Calculate MD5 of file
        /// </summary>
        /// <param name="filename">Name of file on which MD5 calculation will be made</param>
        /// <returns></returns>
        private string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        /// <summary>
        /// Check if file is locked
        /// </summary>
        /// <param name="filename">Name of file on which filelock is checked</param>
        /// <returns>True if locked</returns>
        static bool IsFileLocked(string filename)
        {
            FileInfo file = new FileInfo(filename);
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            //file is not locked
            return false;
        }
    }
}
