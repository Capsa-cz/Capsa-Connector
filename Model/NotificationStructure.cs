using Capsa_Connector.Core.Tools;
using System;

namespace Capsa_Connector.Model
{
    public class NotificationStructure
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Notifications.NotifyTypes? NotifyType { get; set; }
        public Action? callback { get; set; }
    }
}
