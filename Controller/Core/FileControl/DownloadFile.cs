using Capsa_Connector.Controller.Exceptions;
using Capsa_Connector.Core.Helpers;
using Capsa_Connector.Core.Tools;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.FileControl
{
    partial class FileControl
    {
        /// <summary>
        /// Responsible for getting file with all its data from server 
        /// </summary>
        /// <param name="accessToken">accessToken used to specify file on the server</param>
        /// <returns>FilePath to file</returns>
        /// <exception cref="DownloadFileCapsaException">If any error occured inside Task</exception>
        public async Task<string> DownloadFile(string accessToken)
        {
            try
            {
                string tempPath = Path.GetTempPath();

                // Prepare client request
                RestHandler rest = new RestHandler();
                var client = rest.GetRestClient();
                var request = rest.getRestRequest($"files/{accessToken}", Method.Get);
                request.AlwaysMultipartFormData = true;

                // Request response with successfull check
                var response = await client.ExecuteAsync(request);
                if (!StatusCodeChecker.isSuccessful(response.StatusCode, HttpStatusCode.OK))
                    throw new DownloadFileCapsaException($"Unable to download file, Status code: {response.StatusCode}");

                // Check if there are real data
                if (response.RawBytes == null || response.RawBytes.Count() == 0) throw new DownloadFileCapsaException($"No data in response of downloading file");
                byte[] fileBytes = response.RawBytes;

                // Encoding of contentand parsing to correct stuff
                var headervalue = response.ContentHeaders?.ToList().Find(x => x.Name == "Content-Disposition")?.Value;
                string decodedString = WebUtility.UrlDecode((string?)headervalue) ?? throw new DownloadFileCapsaException("Download File -> Problem parsing name");
                string[] headerParts = decodedString.Split(';');
                string? filenamePart = headerParts.FirstOrDefault(part => part.Trim().StartsWith("filename="));
                if (string.IsNullOrEmpty(filenamePart)) throw new DownloadFileCapsaException($"Error DownloadFile -> Filename is empty");

                // Set to correct variables
                string fileName = filenamePart.Split('=')[1].Trim('"');
                string filePath = tempPath + fileName;

                // Write file into filePath
                File.WriteAllBytes(filePath, fileBytes);
                return filePath;
            }
            catch (DownloadFileCapsaException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new DownloadFileCapsaException("Error ocured when downloading file", ex);
            }

        }
    }
}
