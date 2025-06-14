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
        public static async Task<bool> CancelAndUnlockFile(string accessToken)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{accessToken}", Method.Delete);

                var response = await client.ExecuteAsync(request);
                if (response == null)
                {
                    v("CancelAndUnlockFile response is null!!");
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
                v($"Error CancelAndUnlockFile -> {ex.Message}");
                return false;
            }
        }
    }
}
