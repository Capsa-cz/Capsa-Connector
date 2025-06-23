using Windows.Web.AtomPub;

namespace Capsa_Connector.Core
{
    public static class Config
    {
        // Default address configuration
        public static string capsaUrlDefault = "web-1.capsa.cz";
        public static string http = "https://";
        public static string socketProtocol = "wss://";
        public static string capsaUrlForDownloadingApp = "https://www.capsa.cz";
        public static string capsaAppCastUrl = "/cs/appcasts/appcast.xml";
        public static string capsaAppCastPublicKey = "";
        public static string fullAppcastUrl = "https://web-1.capsa.cz/cs/appcasts/appcast.xml";

        // TODO: Move to separate file maybe
        public static string apiPath = "/api/v1/app/";
        public static string uploadFileMethod = "file/upload";
        public static string confirmFileMethod = "file/confirm";
        public static string cancelFileOnlineEditMethod = "file/cancel";
        public static string extendFileLockMethod = "file/lock";
        public static string GetAppTokenMethod = "token";
        public static string defaultWorkspaceLetter = "A";

        // Chunk size
        public static int chunkSize = 1024 * 1024; // (1KB)

        // App behaviour
        public static bool onloadTokenCheck = true;
        public static bool autoStartClientTask = true;
        public static bool obtainNewTokenEveryStart = false;
        public static bool showVersionOnGUI = true;
        public static bool openAlwaysInForeground = true;
        public static bool logRestMethodsSending = true;
        public static bool pingPongLog = true;

        // Startup command config
        public static string startupCommandConfig = "";
        // in secounds
        public static int maxWaitingTimeOfUploadWhenClose = 60;
    }
}
