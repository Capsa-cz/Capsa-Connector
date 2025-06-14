using RestSharp;
using System.Text.RegularExpressions;
using static Capsa_Connector.Core.Tools.Log;
using System;


namespace Capsa_Connector.Core.Helpers
{
    /// <summary>
    /// Custom class to insert local logic for client and request
    /// All constructor info is getting from <c>Config</c> class
    /// </summary>
    class RestHandler
    {
        bool _isLocal = false;
        public RestHandler()
        {
            string pattern = @"\.localhost\b";
            Match m = Regex.Match(StaticVariables.capsaUrl, pattern, RegexOptions.IgnoreCase);
            if (m.Success) { _isLocal = true; }
        }
        /// <summary>
        /// Process data from <c>Config</c> class and use them for creating of <c>RestClient</c> object
        /// </summary>
        /// <returns>Custom RestClient</returns>
        public RestClient GetRestClient()
        {
            RestClientOptions options;
            if (_isLocal)
            {
                // on local -> 127.0.0.1 + set header host
                options = new RestClientOptions(Config.http + "127.0.0.1" + Config.apiPath)
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                    MaxTimeout = int.MaxValue,
                };
            }
            else
            {
                // not local -> Normal addres
                options = new RestClientOptions(Config.http + StaticVariables.capsaUrl + Config.apiPath)
                {
                    MaxTimeout = int.MaxValue
                };
            }
            var client = new RestClient(options);

            return client;
        }

        /// <summary>
        /// Get parameters data, create request and add custom headers to it
        /// </summary>
        /// <param name="appMethod">Should be one of methods from <c>Config</c></param>
        /// <param name="method">HTTP Method</param>
        /// <returns>Custom RestRequest</returns>
        public RestRequest getRestRequest(string appMethod, Method method)
        {
            if (Config.logRestMethodsSending)
            {
                v($"{StaticVariables.capsaUrl} ➡️ {appMethod} [{method}]");
            }

            RestRequest request = new RestRequest(appMethod, method);

            string appToken = Settings1.Default.appToken;

            // Load headers from environment variables
            string origin = Environment.GetEnvironmentVariable("X_CAPSA_ORIGIN") ?? "default-origin";
            string originSecret = Environment.GetEnvironmentVariable("X_CAPSA_ORIGIN_SECRET") ?? "default-secret";

            request.AddHeader("x-capsa-origin", origin);
            request.AddHeader("x-capsa-origin-secret", originSecret);

            if (!string.IsNullOrEmpty(appToken)) request.AddHeader("x-app-token", appToken);
            if (_isLocal) request.AddHeader("Host", StaticVariables.capsaUrl);

            return request;
        }

    }
}
