using Capsa_Connector.Core;
using Capsa_Connector.Core.FileControl;
using Capsa_Connector.Core.FileControlParts;
using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Model;
using Capsa_Connector.ViewModel;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Capsa_Connector.Core.Tools;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.Controller.Core
{
    class ProgramControl
    {
        /// <summary>
        /// Responsible for starting whole new background activity
        /// </summary>
        /// <param name="forceNewAppToken">If true, new app token will be set from server</param>
        /// <param name="mainViewModel">Main View Model</param>
        public async void StartProgram(bool forceNewAppToken = false, MainViewModel? mainViewModel = null)
        {
            try
            {
                v($"-> StartProgram <-");
                // TODO: Force pass mainViewModel instead of passing as StaticRepherence
                if (mainViewModel == null) { mainViewModel = StaticVariables.mainViewModel; }

                // Close other threads
                foreach (var thread in StaticVariables.taskThreadCommands)
                {
                    thread.cancellationTokenSource.Cancel();
                }

                // Get app token from local settings
                string? appToken = Settings1.Default.appToken;
                string? pairingAccessToken = Settings1.Default.pairingAccessToken;

                // Getting new token if there is no token or other options
                if (string.IsNullOrEmpty(appToken) || string.IsNullOrEmpty(pairingAccessToken) || Config.obtainNewTokenEveryStart || forceNewAppToken)
                {
                    // Get token to cancle process if necessary
                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken token = cts.Token;
                    StaticVariables.taskThreadCommands.Add(new TaskThreadCommandsStructure("GetAppTokenTashThread", cts));

                    v($"StartProgram -> Obtaining new token");
                    try
                    {
                        (appToken, pairingAccessToken) = await Task.Run(() => AppToken.GetAppToken(cancellationToken: token, saveToSettings: true));
                    }
                    catch (Exception e)
                    {
                        v($"StartProgram -> Problem obtaining token -> {e.Message} ");
                        if (e.StackTrace != null) v($"StartProgram -> Problem occured when obtaining token -> {e.StackTrace}");
                        return;
                    }


                    // Save app token to local settings
                    Settings1.Default.appToken = appToken;
                    Settings1.Default.pairingAccessToken = pairingAccessToken;
                    Settings1.Default.Save();


                    // Open browser to pair with app
                    v($"StartProgram -> Opening browser for pairing");
                    if (!string.IsNullOrEmpty(pairingAccessToken))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = Config.http + StaticVariables.capsaUrl + "/at/" + pairingAccessToken,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }

                }
                else
                {
                    // Getting token info if token already exist
                    //v($"StartProgram -> Having token -> Getting info");
                    AppTokenInfo? appTokenInfo = await Task.Run(() => AppToken.GetAppTokenInfo(appToken));
                    if (appTokenInfo != null && appTokenInfo.token == appToken)
                    {
                        Settings1.Default.firstName = appTokenInfo?.user?.firstName;
                        Settings1.Default.lastName = appTokenInfo?.user?.lastName;
                        Settings1.Default.email = appTokenInfo?.user?.email;
                        Settings1.Default.Save();
                    }
                }
                mainViewModel.dashboardVM.OnViewModelSet();


                if (StaticVariables.taskThreadCommands.Count != 0)
                {
                    foreach (var threadTask in StaticVariables.taskThreadCommands)
                    {
                        // maybe Dispose here not Cancel
                        threadTask.cancellationTokenSource.Cancel();
                    }
                }


                StaticVariables.taskThreadCommands.Clear();

                if (!string.IsNullOrEmpty(appToken))
                {
                    v($"StartProgram -> Starting Client with app token: {appToken}");
                    StartClient(mainViewModel);
                }
                mainViewModel.dashboardVM.SocketActiveStatus = mainViewModel.dashboardVM.minusmark;
            }

            catch (Exception ex)
            {
                v($"Error StartProgram -> : {ex.Message}");
            }
        }

        /// <summary>
        /// Responsible for starting ClientTaskThread
        /// </summary>
        /// <param name="mainViewModel">Main view model</param>
        public void StartClient(MainViewModel mainViewModel)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            StaticVariables.taskThreadCommands.Add(new Model.TaskThreadCommandsStructure("StartClientTaskThread", cts));

            v($"Startup parameters: {Config.startupCommandConfig}");

            try
            {
                v($"{nameof(StartClient)} -> Starting Client Task");
                _ = Task.Run(() => StartClientTask(token, mainViewModel));
            }
            catch (Exception e)
            {
                v($"Error {nameof(StartClient)} -> Starting Client Task -> {e.Message}");
                mainViewModel?.log(e.Message);
                if (e.StackTrace != null) mainViewModel?.log(e.StackTrace);
                return;
            }
        }

        /// <summary>
        /// Starting websocket, listening to commands
        /// </summary>
        /// <param name="cancellationToken">Cancellation token important for cancellation of thread</param>
        /// <param name="mainViewModel">Main view model</param>
        /// <returns></returns>
        public static async Task StartClientTask(CancellationToken cancellationToken, MainViewModel mainViewModel)
        {
            WebSocketClientHandler webSocketClientHandler = new WebSocketClientHandler();
            ClientWebSocket ws = webSocketClientHandler.WebSocket;

            v($"StartClientTask -> Having WS Client");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    v($"StartClientTask -> Cancellation required -> Code [001]");
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    cancellationToken.ThrowIfCancellationRequested();
                    break;
                }

                try
                {
                    v($"StartClientTask -> Try to connect WS -> {Settings1.Default.appToken}");
                    // this may occure in issues later - need to test when ws is connected (it it can cancle ws com)
                    await ws.ConnectAsync(webSocketClientHandler.Uri, cancellationToken);

                    if (ws.State == WebSocketState.Open) v($"StartClientTask -> WS successfuly connected");
                    else v($"StartClientTask -> WS Code: {ws.State}");

                    using (var ms = new MemoryStream())
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            v($"StartClientTask -> Cancellation required -> Code [002]");
                            await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            cancellationToken.ThrowIfCancellationRequested();
                            break;
                        }
                        while (ws.State == System.Net.WebSockets.WebSocketState.Open)
                        {

                            mainViewModel.dashboardVM.SetIndicatorState(DashboardViewModel.IndicatorSpecifier.Socket, DashboardViewModel.IndicatorState.Successful);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                v($"StartClientTask -> Cancellation required -> Code [003]");
                                await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                cancellationToken.ThrowIfCancellationRequested();
                                mainViewModel.dashboardVM.SetIndicatorState(DashboardViewModel.IndicatorSpecifier.Socket, DashboardViewModel.IndicatorState.Negative);
                                break;
                            }
                            WebSocketReceiveResult result;
                            var buffer = new ArraySegment<byte>(new byte[1024]);
                            do
                            {
                                var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                                result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                            }
                            while (result.Count > 0 && !result.EndOfMessage);

                            var msgString = Encoding.UTF8.GetString(buffer.Array);
                            //var msgString2 = Encoding.UTF8.GetString(messageBuffer);
                            msgString = msgString.Replace("\0", string.Empty);

                            char[] charSeparators = new char[] { '\n' };
                            var responses = msgString.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var responseString in responses)
                            {
                                // check that the response string is json
                                CapsaSocketResponse response;
                                string replacedString = "";
                                try
                                {
                                    replacedString = responseString.Replace("[]", "{}");
                                    response = JsonConvert.DeserializeObject<CapsaSocketResponse>(replacedString);
                                }
                                catch (Exception e)
                                {
                                    mainViewModel.log($"Invalid json - {replacedString}");
                                    mainViewModel.log(e.ToString());
                                    continue;
                                }

                                if (response.command == "ping" && !Config.pingPongLog) v($"↓ {msgString}", false);
                                else v($"↓ {msgString}");

                                switch (response.command)
                                {
                                    case "online-edit-file":
                                        FileControl fileControl = new FileControl(webSocketClientHandler, mainViewModel, response.data.fileAccessToken, response.data.fileURL);
                                        
                                        /*
                                        if (StaticVariables.activeFiles.Any((file =>
                                                file.fileURL == response.data.fileURL)))
                                        {
                                            notify(new NotificationStructure
                                            {
                                                Title = "Soubor je již otevřený",
                                                Text = "Pokud došlo k chybě, zavřete soubor a zkuste znovu",
                                                NotifyType = Notifications.NotifyTypes.Warning
                                            });
                                            // Serialize StaticVariables.activeFiles and log it
                                            mainViewModel.log("Brekekeke");
                                            mainViewModel.log(JsonConvert.SerializeObject(StaticVariables.activeFiles));
                                        }
                                        */
                                        fileControl.StartOnlineEdit();
                                        break;
                                    case "app-token-not-found":
                                        new ProgramControl().StartProgram(forceNewAppToken: true, mainViewModel: mainViewModel);
                                        return;
                                    //appToken = Task.Run(() => AppToken.GetAppToken(saveToSettings: true)).Result.appToken;
                                    case "ping":
                                        await webSocketClientHandler.SendCommand("pong");
                                        StaticVariables.lastPing = DateTime.Now;
                                        break;
                                    case "browser-paired":
                                        v("Browser paired");
                                        if (Settings1.Default.appToken == response.data.appToken)
                                        {
                                            Settings1.Default.paireAppToken = response.data.appToken;
                                            Settings1.Default.Save();
                                        }
                                        mainViewModel.dashboardVM.OnViewModelSet();
                                        break;
                                    default:
                                        v("Socket command not found -> " + response.command);
                                        mainViewModel?.log("Different command received" + response.command);
                                        break;

                                }
                            }
                            // remove commands from memory stream
                            ms.Flush();
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.Position = 0;
                            // sleep 500 ms
                            Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception exception)
                {
                    SetSocketIndication(mainViewModel!, DashboardViewModel.IndicatorState.Unknown);

                    v("Došlo k chybě připojení ke službě WebSocket -> Opětuji pokus", true);
                    v(exception.ToString(), false);
                    
                    SetNewWebSocket(ref ws, ref webSocketClientHandler);

                    StaticVariables.pairCommandSent = default;

                    // wait 10 seconds if not connected
                    Thread.Sleep(10000);
                    continue;
                }
            }
        }

        private static void SetSocketIndication(MainViewModel mainViewModel, DashboardViewModel.IndicatorState indicatorState = DashboardViewModel.IndicatorState.Negative)
        {
            mainViewModel.dashboardVM.SetIndicatorState(DashboardViewModel.IndicatorSpecifier.Socket, indicatorState);
        }

        private static void SetNewWebSocket(ref ClientWebSocket ws, ref WebSocketClientHandler webSocketClientHandler)
        {
            ws.Abort();
            webSocketClientHandler = new WebSocketClientHandler();
            ws = webSocketClientHandler.GetWebSocketClient();
        }
    }
}
