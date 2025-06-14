using Capsa_Connector.Core.Helpers;
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
        /// Need to be send after all chunks are sended to end upload
        /// </summary>
        /// <param name="accessToken">Access token of the file</param>
        /// <param name="hash">Specific hash for closing upload of file to server</param>
        /// <returns></returns>
        static async Task<HttpStatusCode?> CloseFileOnlineEdit(string accessToken, string hash)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{accessToken}/upload/{hash}/close", Method.Patch);

                var response = await client.ExecuteAsync(request);
                if (response == null)
                {
                    v("CloseFileOnlineEdit response is null!!");
                    return null;
                }

                // TODO 419 -> Not standart -> not in normal status code
                return response.StatusCode;
            }
            catch (Exception ex)
            {
                v($"Error CloseFileOnlineEdit -> {ex.Message}");
                return null;
            }
        }
    }
}
