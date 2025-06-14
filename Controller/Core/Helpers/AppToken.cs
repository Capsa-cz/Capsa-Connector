using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.FileControlParts
{
    public class AppToken
    {
        public AppToken() { }
        /// <summary>
        /// Will get tuple of appToken and pairingAccessToken from the server
        /// </summary>
        /// <param name="cancellationToken">Can be used to cancel thread process of getting app token</param>
        /// <param name="saveToSettings">If true, tuple will be saved to app setting storage</param>
        /// <returns>String tuple of app token and pairingAccessToken</returns>
        public static async Task<(string? appToken, string? pairingAccessToken)> GetAppToken(CancellationToken cancellationToken, bool saveToSettings = true)
        {
            // client
            RestHandler rest = new RestHandler();
            var client = rest.GetRestClient();
            var request = rest.getRestRequest(Config.GetAppTokenMethod+"?platform=windows", Method.Get);

            
            string? appToken = null;
            string? pairingAccessToken = null;

            try
            {
                while (string.IsNullOrEmpty(appToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        break;
                    }

                    var requestResponse = await client.ExecuteAsync(request);
                    v(requestResponse.Content);

                    // No nginx server running
                    if (requestResponse == null || requestResponse.StatusCode == 0)
                    {
                        v(requestResponse?.ErrorMessage);
                        v("GetAppToken -> No server / no response");
                    }

                    // Successfull response from server
                    if (StatusCodeChecker.isSuccessful(requestResponse.StatusCode, HttpStatusCode.OK) && requestResponse.Content != null)
                    {
                        //Deserialize response json
                        var capsaTokenResponseWrap = JsonConvert.DeserializeObject<CapsaTokenResponseWrap>(requestResponse.Content);



                        if (capsaTokenResponseWrap != null)
                        {
                            appToken = capsaTokenResponseWrap.data.appToken;
                            pairingAccessToken = capsaTokenResponseWrap.data.pairingAccessToken;
                        }

                        if (saveToSettings && appToken != null && pairingAccessToken != null)
                        {
                            Settings1.Default.appToken = appToken;
                            Settings1.Default.pairingAccessToken = pairingAccessToken;
                            Settings1.Default.Save();
                        }

                        if (appToken != null && pairingAccessToken != null)
                        {
                            StaticVariables.taskThreadCommands.Clear();
                        }
                        return (appToken, pairingAccessToken);
                    }
                    else
                    {
                        v($"{nameof(GetAppToken)} -> Return status code is: {requestResponse.StatusCode}");

                    }
                    await Task.Run(() => { Thread.Sleep(2000); });
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                v("GetAppToken -> requestRespons Expection -> " + ex.Message);
                throw;
            }
        }
        /// <summary>
        /// Will get AppTokenInfo from server to specific <paramref name="appToken"/>
        /// </summary>
        /// <param name="appToken">Valid appToken string</param>
        /// <returns>AppTokenInfo or null</returns>
        public static async Task<AppTokenInfo?> GetAppTokenInfo(string appToken)
        {
            AppTokenInfo? appTokenInfo = null;
            try
            {
                // client
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"{Config.GetAppTokenMethod}/{appToken}", Method.Get);

                // Request response
                var requestResponse = await client.ExecuteAsync(request);

                // Response check
                if (!StatusCodeChecker.isSuccessful(requestResponse.StatusCode, HttpStatusCode.OK))
                { v("GetAppTokenInfo -> request response is not OK"); }
                if (requestResponse.Content == null) throw new Exception("Request content null!");

                // JSON -> XML Objects
                var capsaTokenResponseWrap = JsonConvert.DeserializeObject<AppTokenInfoDataWrap>(requestResponse.Content);
                if (capsaTokenResponseWrap == null) throw new JsonException($"No correct data format of {nameof(AppTokenInfoWrap)} or no data at all!");

                // Unwraping data 
                AppTokenInfoWrap? appTokenData = capsaTokenResponseWrap.data;
                if (appTokenData == null) throw new JsonException($"AppTokenInfo: data are empty {nameof(AppTokenInfoWrap)}");

                // unwraping appTokenInfo
                appTokenInfo = appTokenData?.appToken;
                if (appTokenInfo == null) throw new JsonException($"AppTokenInfo: data are empty {nameof(AppTokenInfo)}");

                // check if app token is null
                if (appTokenInfo == null) throw new Exception("App token is null");
                return appTokenInfo;
            }
            catch (Exception ex)
            {
                v($"Error {nameof(GetAppTokenInfo)} -> {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Get the actual appToken from settings 
        /// </summary>
        /// <returns>Valid appToken string | Null</returns>
        public static string? GetAppTokenFromSettings()
        {
            if (string.IsNullOrEmpty(Settings1.Default.appToken)) return null;
            return Settings1.Default.appToken;
        }
    }

}
