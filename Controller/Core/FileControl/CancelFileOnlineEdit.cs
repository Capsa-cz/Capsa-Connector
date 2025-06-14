using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Core.Tools;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.FileControl
{
    partial class FileControl
    {
        /// <summary>
        /// Send to server cancel online edit command by method delete
        /// </summary>
        /// <param name="accessToken">Access token of active file</param>
        /// <param name="hash">Hash of active online edit</param>
        /// <returns>Bool if server respons was successful</returns>
        public static async Task<bool> CancelFileOnlineEdit(string accessToken, string hash)
        {
            v("CancelFileOnlineEdit -> inside");
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{accessToken}/upload/{hash}", Method.Delete);

                var response = await client.ExecuteAsync(request);
                if (response == null)
                {
                    v("CancelFileOnlineEdit response is null!!");
                    return false;
                }

                if (StatusCodeChecker.isSuccessful(response.StatusCode, HttpStatusCode.NoContent))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                v($"Error CancelFileOnlineEdit -> {ex.Message}");
                return false;
            }
        }
    }
}
