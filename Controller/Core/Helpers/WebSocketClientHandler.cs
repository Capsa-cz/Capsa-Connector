using Capsa_Connector.Model;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.Helpers
{
    /// <summary>
    /// Responsible for providing ClientWebSocket with extra functionality primary for local use
    /// </summary>
    class WebSocketClientHandler
    {
        private readonly bool _local = false;
        public ClientWebSocket WebSocket { get; set; }
        public Uri Uri { get; set; }

        /// <summary>
        /// Use for setup purposes
        /// </summary>
        public WebSocketClientHandler()
        {
            // Local check
            string pattern = @"\.localhost\b";
            Match m = Regex.Match(StaticVariables.capsaUrl, pattern, RegexOptions.IgnoreCase);
            if (m.Success) { _local = true; }

            // WebSocket URI
            if (_local) Uri = new Uri(Config.socketProtocol + "127.0.0.1" + "/ws");
            else { Uri = new Uri(Config.socketProtocol + StaticVariables.capsaUrl + "/ws"); }

            WebSocket = GetWebSocketClient();

        }
        /// <summary>
        /// Will return WebSocketClient and also set new to WebSocketClientHandler if it doesent exist yet.
        /// </summary>
        /// <returns></returns>
        public ClientWebSocket GetWebSocketClient()
        {
            WebSocket = new ClientWebSocket();
            // Local disabling certificate
            if (_local)
            {
                WebSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                WebSocket.Options.SetRequestHeader("Host", StaticVariables.capsaUrl);
            }
            WebSocket.Options.SetRequestHeader("Cookie", $"appToken={Settings1.Default.appToken}");
            WebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            return WebSocket;
        }   

        /// <summary>
        /// Send command throught WebSocketClient to the server
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="dataParams">If null, in command they will be empty</param>
        /// <returns></returns>
        public async Task SendCommand(string commandName, DataCommandContainer? dataParams = null)
        {
            CapsaSocketCommand command = new CapsaSocketCommand();
            command.command = commandName;

            if (dataParams != null) command.data = dataParams;

            // null (error creating) -> {} (working solut)
            else if (dataParams == null)
            {
                command.data = new DataCommandContainer();
                command.data.fileAccessToken = null;
            }

            string commandString = JsonConvert.SerializeObject(command);

            if (commandName == "pong" && !Config.pingPongLog) v($"↑ {commandString}", false);
            else v($"↑ {commandString}");

            // config of sendAsync
            ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(commandString + "\n"), 0, Encoding.UTF8.GetBytes(commandString + "\n").Length);
            WebSocketMessageType messageType = WebSocketMessageType.Text;
            CancellationToken cancellationToken = CancellationToken.None;

            try
            {
                await WebSocket.SendAsync(byteToSend, messageType, true, cancellationToken);
            }
            catch (Exception ex)
            {
                v(ex.Message);
            }
        }
    }
}
