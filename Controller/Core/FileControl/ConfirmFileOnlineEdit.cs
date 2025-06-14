using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using static Capsa_Connector.Core.Tools.Log;


namespace Capsa_Connector.Core.FileControl
{
    partial class FileControl
    {
        static async Task<bool> ConfirmFileOnlineEdit(ActiveFile activeFile)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{activeFile.accessToken}/upload/{activeFile.hash}/confirm", Method.Patch);

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
