using Capsa_Connector.Model;

namespace Capsa_Connector.Controller.Tools
{
    public static class SpecificNotifications
    {
        public static NotificationStructure fileUploadFailed = new NotificationStructure()
        {
            Title = "Nahrávání souboru selhalo",
            Text = "Nepodařilo se nahrát novou verzi souboru",
            NotifyType = Capsa_Connector.Core.Tools.Notifications.NotifyTypes.Error
        };
        public static NotificationStructure notificationTest = new NotificationStructure()
        {
            Title = "TestTitle",
            Text = "TestText",
            NotifyType = Capsa_Connector.Core.Tools.Notifications.NotifyTypes.Warning
        };
        // File conflict
        public static NotificationStructure fileConflict = new NotificationStructure()
        {
            Title = "Konflikt souboru",
            Text = "Soubor byl změněn jiným uživatelem",
            NotifyType = Capsa_Connector.Core.Tools.Notifications.NotifyTypes.Error
        };
    }
}
