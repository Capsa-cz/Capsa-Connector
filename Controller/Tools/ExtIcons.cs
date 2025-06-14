namespace Capsa_Connector.Core.Tools
{
    class ExtIcons
    {

        public static string getIconToExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case "xls":
                case "xlsx":
                case "xlsm":
                    return "/Images/excelus.png";
                case "doc":
                case "docx":
                case "docm":
                    return "/Images/wordus.png";
                case "ppt":
                case "pptx":
                case "pptm":
                    return "/Images/powerpointus.png";
                default:
                    return "/Images/defaultus.png";
            }
        }
    }
}
