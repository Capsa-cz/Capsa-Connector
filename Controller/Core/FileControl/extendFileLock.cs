using Capsa_Connector.Controller.Exceptions;
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
        /// Responsible for extending file lock on call
        /// </summary>
        /// <param name="activeFile"></param>
        /// <exception cref="ExtendFileLockCapsaException">If any problem occured when extending online lock</exception>
        /// <returns>True if successful, false if not able to extend lock</returns>
        static async Task ExtendFileLock(string accessToken)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{accessToken}/lock", Method.Patch);
                request.AlwaysMultipartFormData = true;

                // Sending extend
                var response = await client.ExecuteAsync(request);
                if (response == null)
                {
                    throw new Exception("Client response is null");
                }

                // 2xx -> successfull -> true
                if (StatusCodeChecker.isSuccessful(response.StatusCode, HttpStatusCode.NoContent))
                {
                    return;
                }

                // 409 -> Conflict -> false
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new ExtendFileLockCapsaException("Conflict code", HttpStatusCode.Conflict);
                }

                throw new Exception($"Not expected return from ExtendFileLock -> Code: {response.StatusCode}");
            }
            catch (ExtendFileLockCapsaException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ExtendFileLockCapsaException("Unable to extend online lock", ex);
            }
        }
    }
}
