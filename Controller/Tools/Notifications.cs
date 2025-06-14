using Capsa_Connector.Model;
using System;
using System.Drawing;
using System.Windows.Forms;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.Tools
{
    public static class Notifications
    {
        public enum NotifyTypes
        {
            Info = 1,
            Error = 2,
            Question = 3,
            Warning = 4,
            Shield = 5,
            Default = 6
        }
        public static void ShowNotification(string title, string text, NotifyTypes? notifyType, Action? callback = null)
        {
            if (!Settings1.Default.notificationsEnabled) return;

            Icon BaloonIcon;
            if (notifyType == null) BaloonIcon = SystemIcons.Information;
            else
            {
                switch ((int)notifyType)
                {
                    case 1:
                        BaloonIcon = SystemIcons.Information;
                        break;
                    case 2:
                        BaloonIcon = SystemIcons.Error;
                        break;
                    case 3:
                        BaloonIcon = SystemIcons.Question;
                        break;
                    case 4:
                        BaloonIcon = SystemIcons.Warning;
                        break;
                    case 5:
                        BaloonIcon = SystemIcons.Shield;
                        break;
                    default:
                        BaloonIcon = SystemIcons.Information;
                        break;
                }
            }

            NotifyIcon notifyIcon1 = new NotifyIcon();
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.Icon = BaloonIcon;

            if (callback != null) notifyIcon1.BalloonTipClicked += (e, args) => callback();
            notifyIcon1.ShowBalloonTip(1000);
            notifyIcon1.Dispose();
        }

        public static void notify(NotificationStructure notifyData)
        {
            v($"notify -> title: {notifyData.Title}, descriptions: {notifyData.Text}");
            ShowNotification(notifyData.Title, notifyData.Text, notifyData.NotifyType, notifyData.callback);
        }
    }
}
