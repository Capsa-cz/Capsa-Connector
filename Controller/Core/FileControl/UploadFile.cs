using Capsa_Connector.Controller.Tools;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Capsa_Connector.Controller.Exceptions;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.Core.FileControl
{
    partial class FileControl
    {
        /// <summary>
        /// Responsible for uploading file to server
        /// </summary>
        /// <param name="activeFile">File which will be uploaded</param>
        /// <returns>True if upload was succesfull</returns>
        /// <exception cref="Exception"></exception>
        static async Task<bool> UploadFile(ActiveFile activeFile)
        {
            // uppload active indication

            activeFile.uploadActive = true;
            StaticVariables.uploadActive = true;

            if (activeFile.filePath == null) { throw new Exception("UploadFile file path empty"); }

            string directory;
            string fileName;
            string extension;
            string copyFilePath;
            try
            {
                // Create file structure and directory place stuff
                directory = Path.GetDirectoryName(activeFile.filePath) ?? throw new Exception("GetDirectoryName problem");
                fileName = Path.GetFileNameWithoutExtension(activeFile.filePath);
                extension = Path.GetExtension(activeFile.filePath);
                copyFilePath = directory + "\\" + fileName + "-copy" + extension;
            }
            catch (Exception ex)
            {
                throw new Exception("Upload -> Problem occured when getting Paths -> " + ex.Message);
            }


            // Create copy for be able to upload active file (you cant do it differently)
            if (File.Exists(copyFilePath) && !IsFileLocked(copyFilePath))
                File.Delete(copyFilePath);


            // Solves problem with XLS (app usage of files) - it will leave this file open until it is free
            while (true)
            {
                try
                {
                    File.Copy(activeFile.filePath, copyFilePath);
                    break;
                }
                catch (Exception ex)
                {
                    v(ex.Message);
                    Thread.Sleep(20);
                    continue;
                }
            }

            // Send data as chunks
            bool successfullUpload = true;
            using (FileStream fileStream = new FileStream(copyFilePath, FileMode.Open, FileAccess.Read))
            {
                long fileSize = fileStream.Length;
                long bytesRemaining = fileSize;

                string? hash = Task.Run(async () => await CreateFileOnlineEdit(fileSize, activeFile.md5Hash, activeFile)).Result;
                if (hash == null) throw new Exception("Unable to get hash for upload");
                activeFile.hash = hash;

                v($"UploadFile -> Hash -> {hash}");

                do
                {
                    int bufferSize = (int)Math.Min(Config.chunkSize, bytesRemaining);
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead = fileStream.Read(buffer, 0, bufferSize);

                    v("Actual chungus size is: " + bufferSize);

                    long uploadedSize = fileSize - bytesRemaining;

                    v("UPLOADED SIZE: " + uploadedSize);
                    v("CHUNK SIZE:" + bufferSize);

                    bool chunkSended = false;
                    int tryCount = 0;

                    while (!chunkSended)
                    {
                        try
                        {
                            HttpStatusCode? responseCode = Task.Run(async () => await AppendFileOnlineEdit(bufferSize, uploadedSize, buffer, activeFile.accessToken, hash)).Result;
                            v($"AppendCode: {responseCode}");

                            switch (responseCode)
                            {
                                case null:
                                    throw new Exception("Problem appending data, response code is null");
                                    break;
                                case HttpStatusCode.OK:
                                    chunkSended = true;
                                    break;
                                case HttpStatusCode.Unauthorized:
                                    notify(SpecificNotifications.fileUploadFailed);
                                    successfullUpload = false;
                                    break;
                                case HttpStatusCode.Forbidden:
                                    notify(SpecificNotifications.fileUploadFailed);
                                    successfullUpload = false;
                                    return false;
                                    break;
                                case HttpStatusCode.InternalServerError:
                                    if (tryCount > 10) return false;
                                    tryCount++;
                                    Thread.Sleep(1000);
                                    continue;
                                    break;
                                case HttpStatusCode.BadRequest:
                                    activeFile.uploadAgaing = true;
                                    return false;
                                    break;
                                default:
                                    throw new Exception("Bad response code: " + responseCode);
                            }
                        }
                        catch (AppendFileCapsaException ex)
                        {
                            v("Error while appending file -> " + ex.Message);
                            continue;
                        }
                        catch (Exception ex)
                        {
                            v("Uncatched error occured when appending file -> " + ex.Message);
                            return false;
                        }
                        if(!chunkSended && tryCount > 10)
                        {
                            return false;
                        }
                    }

                    bytesRemaining -= bytesRead;
                }
                while (bytesRemaining > 0);
            }
            if (successfullUpload)
            {
                HttpStatusCode? closeCode = Task.Run(async () => await CloseFileOnlineEdit(activeFile.accessToken, activeFile.hash)).Result;
                if (closeCode == null) { throw new Exception("UploadFile -> CloseFileOnlineEdit -> Code:null"); }

                v($"CloseFileOnlineEdit -> Code: {closeCode}");

                if (StatusCodeChecker.isSuccessful(closeCode, HttpStatusCode.NoContent))
                {
                    activeFile.uploadSuccessfull = true;
                    activeFile.wasUpdated = true;
                }
                //  TODO: 419 -> Wrong md5 -> whole again
                else if (closeCode == HttpStatusCode.Locked)
                {
                    // Uploaded, close was alredy called -> My problem
                    v("Was closed before!!!");
                }
                else
                {
                    notify(SpecificNotifications.fileUploadFailed);
                    bool wasCancelled = Task.Run(async () => await CancelFileOnlineEdit(activeFile.accessToken, activeFile.hash)).Result;
                    // TODO: End file edit
                    if (wasCancelled) v("Edit was cancelled");
                }
            }
            else
            {
                bool wasCancelled = Task.Run(async () => await CancelFileOnlineEdit(activeFile.accessToken, activeFile.hash)).Result;
                if (wasCancelled) v("Edit was cancelled");
            }

            activeFile.lastSynced = DateTime.Now;

            // Solves problem with XLS (app usage of files) - it will leave this file open until it is free
            while (true)
            {
                try
                {
                    File.Delete(copyFilePath);
                    break;
                }
                catch (Exception ex)
                {
                    v(ex.Message);
                    Thread.Sleep(20);
                    continue;
                }
            }

            // File not uploading indication
            activeFile.uploadActive = false;
            return true;
        }
    }
}
