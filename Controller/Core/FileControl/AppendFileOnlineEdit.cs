using Capsa_Connector.Core.Helpers;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;
using Capsa_Connector.Controller.Exceptions;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.FileControl
{
    partial class FileControl
    {
        /// <summary>
        /// Responsible for sending data to server and returning status code to code or null
        /// </summary>
        /// <param name="chunkSize">Size of chunk in bytes</param>
        /// <param name="uploadedSize">Size uploaded before this chunk</param>
        /// <param name="byteChunk">Data of chunk</param>
        /// <param name="accessToken">Access token to the file</param>
        /// <param name="hash">Specific hash for appending to file on server</param>
        /// <returns>Status code of upload or null when problem occured on client on app side</returns>
        static async Task<HttpStatusCode?> AppendFileOnlineEdit(int chunkSize, long uploadedSize, byte[] byteChunk, string accessToken, string hash)
        {
            try
            {
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{accessToken}/upload/{hash}/append", Method.Patch);

                request.AddHeader("X-Chunk-Size", chunkSize);
                request.AddHeader("X-Uploaded-Size", uploadedSize);

                //string encodedByteChunk = Encoding.UTF8.GetString(byteChunk);
                //request.AddParameter("text/plain", encodedByteChunk, ParameterType.RequestBody);

                request.AddParameter("application/octet-stream", byteChunk, ParameterType.RequestBody);

                var response = await client.ExecuteAsync(request);
                v(response.Content);

                if (response == null)
                {
                    v("AppendFileOnlineEdit response is null");
                    return null;
                }

                return response.StatusCode;
            }
            catch (Exception ex)
            {
                v($"Error AppendFileOnlineEdit -> {ex.Message}");
                throw new AppendFileCapsaException("Error while appending file", ex);
            }
        }
    }
}
