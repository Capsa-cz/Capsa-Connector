using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Capsa_Connector.Core.Tools
{
    public class Log
    {
        string localAddress = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CapsaConnector") + "//log";

        // Directories to write on specific log
        private string[] all =
        {
            "/debug.txt",
            "/information.txt",
            "/warning.txt",
            "/error.txt",
            "/critical.txt",
            "/all.txt"
        };
        private string[] debug = { "//debug.txt" };
        private string[] information = { "//information.txt" };
        private string[] warning = { "//warning.txt" };
        private string[] error = { "//error.txt" };
        private string[] critical = { "//critical.txt" };


        public Log()
        {
            if (!Directory.Exists(localAddress)) { DirectoryInfo di = Directory.CreateDirectory(localAddress); };
        }

        public static void DumpLogFilesIfLarge()
        {
            try
            {
                string logDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CapsaConnector") + "//log";
                string[] logFiles = Directory.GetFiles(logDirectory, "*.txt");

                foreach (string logFile in logFiles)
                {
                    FileInfo fileInfo = new FileInfo(logFile);
                    if (fileInfo.Length > 1024 * 1024 * 5) // 5 MB
                    {
                        File.WriteAllText(logFile, string.Empty); // Clear the original log file
                    }
                }
            }
            catch (DirectoryNotFoundException e)
            {
                v("Log directory not found: " + e.Message);
            }
            catch (Exception e)
            {
                // TODO: report
                v("An error occurred while clearing log files: " + e.Message);
            }
            
        }
        
        public static void v(string message, bool isInConsole = true)
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("HH:mm:ss | dd.MM.yyyy | ");

            message = $"{formattedDateTime}{message}";
            string viewConsoleLine = $"{message}{"\n"}";

            if (isInConsole && StaticVariables.mainViewModel != null) 
                StaticVariables.mainViewModel.log(viewConsoleLine);

            new Log().Debug(viewConsoleLine);

        }
        private void Debug(string message)
        {
            foreach (var item in debug)
            {
                LogIntoFile(item, message);
            }
        }
        private void Inforamtion(string message)
        {
            foreach (var item in information) { LogIntoFile(item, message); }
        }
        private void Warning(string message)
        {
            foreach (var item in warning) { LogIntoFile(item, message); }
        }
        private void Error(string message)
        {
            foreach (var item in error) { LogIntoFile(item, message); }
        }
        private void Critical(string message)
        {
            foreach (var item in critical) { LogIntoFile(item, message); }
        }

        private void LogIntoFile(string fileName, string messageLine)
        {
            while (true)
            {
                try
                {
                    lock (this)
                    {
                        if (!Directory.Exists(localAddress))
                            Directory.CreateDirectory(localAddress);

                        if (!File.Exists(localAddress + fileName))
                            File.Create(localAddress + fileName).Close();

                        try
                        {
                            using (StreamWriter sw = File.AppendText(localAddress + fileName))
                            {
                                sw.WriteLine(messageLine);
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString());
                            break;
                        }
                    }
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
