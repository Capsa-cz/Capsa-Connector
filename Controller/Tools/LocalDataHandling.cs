using Capsa_Connector.Controller.Exceptions;
using Capsa_Connector.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Markup;

namespace Capsa_Connector.Core.Tools
{
    public static class LocalDataHandling
    {
        public static List<DashboardElementStructure> getFileHistory()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CapsaConnector");
            string filePath = folderPath + "\\fileHistory.txt";
            string content;

            if (!File.Exists(filePath))
            {
                return new List<DashboardElementStructure>();
            }

            try
            {
                content = File.ReadAllText(filePath);
                if (content == null || content == "")
                {
                    return new List<DashboardElementStructure>();
                }

                List<DashboardElementStructure>? data = JsonSerializer.Deserialize<List<DashboardElementStructure>>(content);
                if (data == null)
                {
                    return new List<DashboardElementStructure>();
                }
                foreach (DashboardElementStructure element in data)
                {
                    element.ImageToPreviewImage = ExtIcons.getIconToExtension(element.FileExtension ?? "");
                }
                return data;
            }
            catch (Exception ex)
            {
                throw new ReadingLocalEditHistoryCapsaException("Error deserialize data from user file", ex);
            }
        }

        public static void saveFileHistory(List<DashboardElementStructure> data)
        {
            string content = JsonSerializer.Serialize(data);

            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CapsaConnector");
            string filePath = folderPath + "\\fileHistory.txt";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            File.WriteAllText(filePath, content);
        }

        public static void removeFileHistory()
        {
            List<DashboardElementStructure> data = new List<DashboardElementStructure>();
            saveFileHistory(data);
        }
    }
}
