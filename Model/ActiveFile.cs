using System;
using System.IO;

namespace Capsa_Connector.Model
{
    public class ActiveFile
    {
        public string accessToken;
        public string? filePath;
        public DateTime lastSynced;
        public DateTime lockExtended;
        public FileSystemWatcher? watcher;
        public bool fileOpenedSended;

        // Specific for uploading state
        public bool uploadActive = false;
        public bool uploadSuccessfull = false;
        public bool wasUpdated = false;
        public bool wasCommitted = false;
        public bool uploadAgaing = false;
        public bool hasLocking = false;
        public bool errorOnUpload = false;

        public string? md5Hash;
        public string fileURL;
        public string? hash;
        public int fileSize;
        public string fileName;

        // Create a class constructor for the activeFile class
        public ActiveFile(string newaccessToken, string webFileURL)
        {
            accessToken = newaccessToken;
            fileURL = webFileURL;
        }
    }
}
