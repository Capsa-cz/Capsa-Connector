using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Capsa_Connector.Controller.Tools;
using Capsa_Connector.Core;
using Capsa_Connector.Core.Bases;
using Capsa_Connector.Core.FileControlParts;
using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using Capsa_Connector.View;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.Controller.Core.Helpers
{
    public class DiskHandler
    {
        private string? email;
        private string? diskDomain;
        private string? sshfsAppToken;
        private RelayCommand? updateGuiRelayCommand;

        public void SetEmail(string? email) => this.email = email;
        
        public void SetUpdateGuiRelayCommand(RelayCommand updateGuiRelayCommand) => this.updateGuiRelayCommand = updateGuiRelayCommand;

        /// <summary>
        /// Returns the letter of the connected disk for the given workspace key.
        /// </summary>
        /// <param name="workspaceKey"></param>
        /// <returns> The letter of the connected disk.</returns>
        /// <exception cref="GetLetterOfConnectedDiskException"></exception>
        private string GetWorkspaceLetter(string workspaceKey)
        {
            try
            {
                string command = $"cmd /V:OFF /C \"net use | findstr /C:\"\\\\sshfs\\{email}@{diskDomain}!!1999\\{sshfsAppToken}\\{workspaceKey}\"\"";
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = psi };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();

                string[] parts = output.Split(':');
                return parts[0].Trim();
            }
            catch (Exception e)
            {
                LogError($"Error in GetWorkspaceLetter: {e.Message}");
                throw new GetLetterOfConnectedDiskException(e.Message);
            }
        }

        /// <summary>
        /// Returns true if the workspace is connected, false otherwise.
        /// </summary>
        /// <param name="workspaceKey"></param>
        /// <returns></returns>
        private bool IsWorkspaceConnected(string workspaceKey)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                string command = "net use";
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.GetEncoding(850)
                };

                using Process process = new Process { StartInfo = psi };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string[] lines = output.Split('\n')
                    .Select(line => line.Trim())
                    .Where(line => line.Contains(@"\\sshfs\"))
                    .ToArray();

                string pattern = $@"\\\\sshfs\\{Regex.Escape(email)}@{Regex.Escape(diskDomain)}!!1999\\[^\\]+\\{Regex.Escape(workspaceKey)}";
                v("Pattern: " + pattern);
                v("Output" + output);

                foreach (string line in lines)
                {
                    Console.WriteLine($"LINE: {line}");
                    if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase))
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                LogError($"Error in IsWorkspaceConnected: {e.Message}");
                return false;
            }
        }




        /// <summary>
        /// This method will connect the disk to the system. It will connect to disk letter using net use command and other important parameters for connecting to network drive.
        /// </summary>
        /// <param name="workspace"></param>
        /// <exception cref="MissingValuesForDiskConnection"></exception>
        /// <exception cref="DiskConnectionException"></exception>
        private async void ConnectToWorkspace(Workspace workspace)
        {
            try
            {
                //Get from settings if workspace (disk) renaming is enabled
                bool workspaceRenaming = Settings1.Default.DiskRenaming;
                string password = "";
                SetEmail(null);
                if (email == null)
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    AppTokenInfo? appTokenInfo =
                        await Task.Run(() => AppToken.GetAppTokenInfo(Settings1.Default.appToken));
                    SetEmail(appTokenInfo.user.email);
                    Mouse.OverrideCursor = null;

                    if (string.IsNullOrEmpty(email))
                    {
                        throw new MissingValuesForDiskConnection("Email is not set.");
                    }
                }

                bool connectButtonWasClicked = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var loginWindow = new DiskConnectWindow(email, workspace.Title);
                    if (loginWindow.ShowDialog() == true)
                    {
                        password = loginWindow.Password;
                        workspace.WorkspaceLetter = loginWindow.DiskLetter;
                        connectButtonWasClicked = loginWindow.ButtonClicked;
                    }
                    else
                    {
                        return;
                    }
                });
                if (!connectButtonWasClicked) return;

                if (string.IsNullOrEmpty(workspace.WorkspaceLetter) || String.IsNullOrEmpty(password))
                {
                    throw new MissingValuesForDiskConnection("Password or disk letter is not set.");
                }

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                string perzistantDiskDommand = "net use /p:yes";
                v($"Setting persistent disk command: {perzistantDiskDommand}");
                ExecuteCommand(perzistantDiskDommand);
                string connectCommand =
                    $"net use {workspace.WorkspaceLetter}: \\\\sshfs\\{email}@{diskDomain}!!1999\\{sshfsAppToken}\\{workspace.WorkspaceKey} /user:{email} {password} /persistent:yes";
                v(connectCommand);
                ExecuteCommand(connectCommand);

                bool mountReady = await WaitForMountReady(workspace.WorkspaceLetter);
                if (!mountReady)
                {
                    LogError("Mountpoint nebyl připraven včas.");
                    return;
                }

                try
                {
                    CredentialManager.SaveCredential(
                        $@"\\sshfs\{email}@{diskDomain}!!1999\{sshfsAppToken}\{workspace.WorkspaceKey}", email, password);
                }
                catch (Exception e)
                {
                    v(e.Message);
                }
                
                string mountPointName =
                    $"##sshfs#{email}@{diskDomain}!!1999#{sshfsAppToken}#{workspace.WorkspaceKey}".Replace("\\", "");

                if (workspaceRenaming)
                {
                    SetMountPointLabel(mountPointName, workspace.Title);
                }
                
                Mouse.OverrideCursor = null;
                updateGuiRelayCommand?.Execute(null);
                
                // Try to open the disk in File Explorer
                string path = $"{workspace.WorkspaceLetter}:\\";
                if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
                else
                {
                    LogError($"Directory {path} does not exist after mounting.");
                }
                
                await Task.Delay(5000);
                if (workspaceRenaming)
                {
                    SetMountPointLabel(mountPointName, workspace.Title);
                }                
                await Task.Delay(60000);
                if (workspaceRenaming)
                {
                    SetMountPointLabel(mountPointName, workspace.Title);
                }            
            }
            catch (MissingValuesForDiskConnection ex)
            {
                LogError($"Missing values for disk connection: {ex.Message}");
                notify(new NotificationStructure()
                {
                    Title = "Připojení disku",
                    Text = "Disk se nepodařilo připojit.",
                    NotifyType = NotifyTypes.Error
                });
                Mouse.OverrideCursor = null;
            }
            catch (SettingMountPointLabelException ex)
            {
                notify(new NotificationStructure()
                {
                    Title = "Pojmenování disku",
                    Text = "Nepodařilo se správně pojmenovat disk.",
                    NotifyType = NotifyTypes.Error
                }); 
                v($"Error setting mount point label: {ex.Message}");
                // Try to call SetMountPointLabel again, but with 5s delay
                await Task.Delay(5000);
                try
                {
                    string mountPointName =
                        $"##sshfs#{email}@{diskDomain}!!1999#{sshfsAppToken}#{workspace.WorkspaceKey}".Replace("\\", "");
                    v(mountPointName);
                    SetMountPointLabel(mountPointName, workspace.Title);
                }
                catch (Exception innerEx)
                {
                    LogError($"Error setting mount point label after delay: {innerEx.Message}");
                }
                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                LogError($"Error connecting to disk: {ex.Message}");
                //v(ex.StackTrace);
                notify(new NotificationStructure()
                {
                    Title = "Nepodařilo se připojit",
                    Text = "Zkontrolujte si přihlašovací údaje.",
                    NotifyType = NotifyTypes.Error
                });
                Mouse.OverrideCursor = null;
            }
        }
        
        private async Task<bool> WaitForMountReady(string driveLetter, int timeoutMs = 10000)
        {
            var stopwatch = Stopwatch.StartNew();
            string path = $"{driveLetter}:\\";
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    if (Directory.Exists(path)) return true;
                }
                catch
                {
                    // Ignore exceptions, just wait and retry
                }
                await Task.Delay(500);
            }
            return false;
        }

        /// <summary>
        /// This method will disconect the disk from the system, depending on workspace.WorkspaceLetter.
        /// </summary>
        /// <param name="workspace"></param>
        /// <exception cref="DiskNotConnectedException"></exception>
        /// <exception cref="DiskLetterNotFoundedException"></exception>
        /// <exception cref="ErrorDisconnectingDiskException"></exception>
        private void DisconnectDisk(Workspace workspace)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                if (!IsWorkspaceConnected(workspaceKey: workspace.WorkspaceKey))
                {
                    throw new DiskNotConnectedException("Disk is not connected.");
                }

                if (string.IsNullOrEmpty(workspace.WorkspaceLetter))
                {
                    throw new DiskLetterNotFoundedException("Workspace letter is not set.");
                }

                string workspaceLetter = workspace.WorkspaceLetter;
                if(workspace.WorkspaceLetter.Length != 1) workspaceLetter = workspace.WorkspaceLetter.Substring(workspace.WorkspaceLetter.Length - 1);
                v($"Disconnecting disk with letter: {workspaceLetter}");
                v($"Executing command: net use {workspaceLetter}: /delete");
                ExecuteCommand($"net use {workspaceLetter}: /delete");
                
                Mouse.OverrideCursor = null;
            }
            catch (DiskNotConnectedException ex)
            {
                LogError($"Disk not connected: {ex.Message}");
                Mouse.OverrideCursor = null;
                throw;
            }
            catch (DiskLetterNotFoundedException ex)
            {
                LogError($"Disk letter not found: {ex.Message}");
                Mouse.OverrideCursor = null;
                throw;
            }
            catch (Exception ex)
            {
                LogError($"Error in DisconnectDisk: {ex.Message}");
                Mouse.OverrideCursor = null;
                throw new ErrorDisconnectingDiskException($"Error disconnecting disk: {ex.Message}");
            }
        }

        /// <summary>
        /// Helpers method to execute command in cmd.
        /// </summary>
        /// <param name="command"></param>
        /// <exception cref="Exception"></exception>
        private void ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = psi };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in ExecuteCommand: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sets the label of the mount point in the registry.
        /// </summary>
        /// <param name="mountPointName"></param>
        /// <param name="label"></param>
        /// <exception cref="SettingMountPointLabelException"></exception>
        private void SetMountPointLabel(string mountPointName, string label)
        {
            v("Trying to set mount point label... with " + mountPointName + " and label: " + label);
            try
            {
                string keyPath = $@"Software\Microsoft\Windows\CurrentVersion\Explorer\MountPoints2\{mountPointName}";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("_LabelFromReg", label, RegistryValueKind.String);
                    }
                    else
                    {
                        v($"Registry key {keyPath} not found.");
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"Error in SetMountPointLabel: {e.Message}");
                throw new SettingMountPointLabelException(e.Message);
            }
        }

        /// <summary>
        /// This method will get the available workspaces from the server using the app token.
        /// </summary>
        /// <param name="appToken"></param>
        /// <param name="updateGuiRelayCommand"></param>
        /// <returns></returns>
        /// <exception cref="UnableToGetAvailableDiskException"></exception>
        /// <exception cref="DiskConnectionException"></exception>
        /// <exception cref="CantGetAvailableWorkspaces"></exception>
        public async Task<List<WorkspaceDashboardStructure>?> GetAvailableWorkspaces(string appToken, RelayCommand updateGuiRelayCommand)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"{Config.http}{StaticVariables.capsaUrl}{Config.apiPath}sshfs-app-token/{appToken}", Method.Get);

                var response = await client.ExecuteAsync(request);
                if (!StatusCodeChecker.isSuccessful(response.StatusCode, HttpStatusCode.OK))
                    throw new UnableToGetAvailableDiskException($"Unable to get available disks, Status code: {response.StatusCode}");

                if (response.Content == null) throw new UnableToGetAvailableDiskException("Response content is null");

                var diskResponse = JsonConvert.DeserializeObject<DiskResponse>(response.Content);
                if (diskResponse?.Data?.Workspaces == null) throw new UnableToGetAvailableDiskException("No workspaces found");

                sshfsAppToken = diskResponse.Data?.SshfsAppToken?.Token;
                diskDomain = diskResponse.Data?.DiskDomain ?? string.Empty;

                var workspacesList = diskResponse.Data?.Workspaces.Select(ws => new WorkspaceDashboardStructure
                {
                    Key = ws.WorkspaceKey,
                    Title = ws.Title,
                    IsConnected = IsWorkspaceConnected(ws.WorkspaceKey),
                    Letter = GetWorkspaceLetter(ws.WorkspaceKey) ?? null,
                    ActionCommand = new RelayCommand(o =>
                    {
                        if (!IsWorkspaceConnected(ws.WorkspaceKey))
                        {
                            try
                            {
                                ConnectToWorkspace(ws);
                                ws.IsConnected = true;
                            }
                            catch (Exception ex)
                            {
                                LogError($"Error connecting to disk: {ex.Message}");
                                //throw new DiskConnectionException($"Error connecting to disk: {ex.Message}");
                            }
                        }
                        else
                        {
                            try
                            {
                                ws.WorkspaceLetter = GetWorkspaceLetter(ws.WorkspaceKey);
                                DisconnectDisk(ws);
                                ws.IsConnected = false;
                            }
                            catch (Exception ex)
                            {
                                LogError($"Error disconnecting disk: {ex.Message}");
                                ws.IsConnected = true;
                                //throw new DiskConnectionException($"Error disconnecting disk: {ex.Message}");
                            }
                        }
                        updateGuiRelayCommand.Execute(null);
                    })
                }).ToList();

                return workspacesList;
            }
            catch (UnableToGetAvailableDiskException ex)
            {
                LogError($"Unable to get available disks: {ex.Message}");
                throw;
            }
            catch (DiskConnectionException ex)
            {
                LogError($"Disk connection error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                LogError($"Error in GetAvailableWorkspaces: {ex.Message}");
                throw new CantGetAvailableWorkspaces($"Error getting available workspaces: {ex.Message}");
            }
        }

        private void LogError(string message)
        {
            v(message);
        }
    }

    // Structures for disk connection
    public class DiskResponse
    {
        public DiskData? Data { get; set; }
    }
    public class DiskData
    {
        public string? DiskDomain { get; set; }
        public SshfsAppToken? SshfsAppToken { get; set; }
        public Workspace[]? Workspaces { get; set; }
    }
    public class SshfsAppToken
    {
        public string? Token { get; set; }
    }
    public class Workspace
    {
        public string WorkspaceKey { get; set; }
        public string Title { get; set; }
        public bool IsConnected { get; set; }
        public string WorkspaceLetter { get; set; }
    }

    // Exceptions for disk connection
    public class DiskAlreadyConnectedException : Exception
    {
        public DiskAlreadyConnectedException(string message) : base(message)
        {
        }
    }   
    public class DiskConnectionException : Exception
    {
        public DiskConnectionException(string message) : base(message)
        {
        }
    }
    public class DiskNotConnectedException : Exception
    {
        public DiskNotConnectedException(string message) : base(message)
        {
        }
    }
    public class DiskLetterNotFoundedException : Exception
    {
        public DiskLetterNotFoundedException(string message) : base(message)
        {
        }
    }
    public class SettingMountPointLabelException : Exception
    {
        public SettingMountPointLabelException(string message) : base(message)
        {
        }
    }
    public class ErrorDisconnectingDiskException : Exception
    {
        public ErrorDisconnectingDiskException(string message) : base(message)
        {
        }
    }
    public class CantGetAvailableWorkspaces : Exception
    {
        public CantGetAvailableWorkspaces(string message) : base(message)
        {
        }
    }
    public class MissingValuesForDiskConnection : Exception
    {
        public MissingValuesForDiskConnection(string message) : base(message)
        {
        }
    }
    public class GetLetterOfConnectedDiskException : Exception
    {
        public GetLetterOfConnectedDiskException(string message) : base(message)
        {
        }
    }
    public class UnableToGetAvailableDiskException : Exception
    {
        public UnableToGetAvailableDiskException(string message) : base(message)
        {
        }
    }
}
