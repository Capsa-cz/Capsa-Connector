using Capsa_Connector.Controller.Tools;
using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.Core.FileControl
{
    partial class FileControl
    {
        /// <summary>
        /// Creates file in online edit before sending chunks
        /// <para>Method: files/{accessToken}</para>
        /// </summary>
        /// <param name="activeFile"></param>
        /// <returns>Hash for chunk sending, null if problem occured</returns>
        static async Task<string?> CreateFileOnlineEdit(long fileSize, string checksum, ActiveFile activeFile)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{activeFile.accessToken}", Method.Post);

                ContentOfUploadCreate content = new ContentOfUploadCreate
                {
                    size = fileSize,
                    checksum = checksum
                };

                request.AddBody(JsonConvert.SerializeObject(content));

                var response = await client.ExecuteAsync(request);

                if (response == null || response.Content == null)
                {
                    v("CreateFileOnlineEdit response is null or content is null!");
                    return null;
                }

                // 201 -> return hash
                if (StatusCodeChecker.isSuccessful(response.StatusCode, HttpStatusCode.Created))
                {
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        if (response.Content == null) { return null; }
                        return JsonConvert.DeserializeObject<ContentDataString>(response.Content)?.data ?? null;
                    }
                    v("CreateFileOnlineEdit -> response.Content isNullOrEmpty");
                }

                // 403, 401 -> no access (notification for baba)
                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    notify(SpecificNotifications.fileUploadFailed);
                    return null;
                }

                if(response.StatusCode == HttpStatusCode.Conflict)
                {
                    notify(SpecificNotifications.fileConflict);
                    return null;
                }
                return null;

            }
            catch (Exception ex)
            {
                v($"Error CreateFileOnlineEdit -> {ex.Message}");
                return null;
            }
        }
    }
}
