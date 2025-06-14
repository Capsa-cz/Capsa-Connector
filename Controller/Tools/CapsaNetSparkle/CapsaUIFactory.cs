using System;
using System.Collections.Generic;
using Capsa_Connector.View;
using Capsa_Connector.ViewModel;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.WPF;
using static Capsa_Connector.Core.Tools.Log;
using static Capsa_Connector.Core.Tools.Notifications;

namespace Capsa_Connector.Controller.Tools.CapsaNetSparkle
{

    public class CapsaUIFactory : NetSparkleUpdater.Interfaces.IUIFactory
    {
        public IUpdateAvailable CreateUpdateAvailableWindow(List<AppCastItem> updates, ISignatureVerifier? signatureVerifier,
            string currentVersion = "", string appName = "the application", bool isUpdateAlreadyDownloaded = false)
        {
            AvailableUpdateViewModel updateAvailableWindowViewModel = new AvailableUpdateViewModel(updates[0]);
            AvailableUpdateWindow updateAvailableWindow = new AvailableUpdateWindow(updateAvailableWindowViewModel);

            if (HideReleaseNotes)
            {
                ((IUpdateAvailable)updateAvailableWindow).HideReleaseNotes();
            }

            if (HideSkipButton)
            {
                ((IUpdateAvailable)updateAvailableWindow).HideSkipButton();
            }

            if (HideRemindMeLaterButton)
            {
                ((IUpdateAvailable)updateAvailableWindow).HideRemindMeLaterButton();
            }

            return updateAvailableWindow;
        }

        public IDownloadProgress CreateProgressWindow(string downloadTitle, string actionButtonTitleAfterDownload)
        {
            var window = new DownloadUpdateWindow(downloadTitle, actionButtonTitleAfterDownload);
            return window;
        }

        public ICheckingForUpdates ShowCheckingForUpdates()
        {
            var window = new CheckingForUpdatesWindow();
            return window;
        }

        public void ShowVersionIsUpToDate()
        {
            notify(new Model.NotificationStructure
            {
                Title = "Aktualizace",
                Text = "Vaše verze je aktuální",
                NotifyType = NotifyTypes.Info,
                callback = () =>
                {
                    
                }
            });
            v("Version check -> Version is actual");
            throw new NotImplementedException();
        }

        public void ShowVersionIsSkippedByUserRequest()
        {
            notify(new Model.NotificationStructure
            {
                Title = "Aktualizace",
                Text = "Aktualizace byla přeskočena uživatelsky",
                NotifyType = NotifyTypes.Info,
                callback = () =>
                {
                    
                }
            });
            v("Version check -> Version is skipped by user");
        }

        public void ShowCannotDownloadAppcast(string? appcastUrl)
        {
            notify(new Model.NotificationStructure
            {
                Title = "Chyba aktualizace",
                Text = "Nastal problém při stahování aktualizace!",
                NotifyType = NotifyTypes.Warning,
                callback = () =>
                {
                    // TODO: Dodělat nahlášení chyby nebo něco takového
                }
            });
            v("Appcast download problem -> " + appcastUrl);
        }

        public bool CanShowToastMessages()
        {
            return false;
        }

        public void ShowToast(Action clickHandler)
        {
            notify(new Model.NotificationStructure
            {
                Title = "Nová verze aplikace",
                Text = "Capsa Connector dostal aktualizaci!",
                NotifyType = NotifyTypes.Info,
                callback = () =>
                {
                    // TODO: Implementace otevření aktualizačního okna a zahájení loopu
                }
            });
        }

        public void ShowDownloadErrorMessage(string message, string? appcastUrl)
        {
            notify(new Model.NotificationStructure
            {
                Title = "Chyba aktualizace",
                Text = "Nastal problém při stahování aktualizace!",
                NotifyType = NotifyTypes.Warning,
                callback = () =>
                {
                    // TODO: Dodělat nahlášení chyby nebo něco takového
                }
            });
            v("Download error message -> " + message);
        }

        public void Shutdown()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public bool HideReleaseNotes { get; set; }

        public bool HideSkipButton { get; set; }

        public bool HideRemindMeLaterButton { get; set; }

        public string ReleaseNotesHTMLTemplate { get; set; }

        public string AdditionalReleaseNotesHeaderHTML { get; set; }
        public void Init(SparkleUpdater sparkle)
        {
        }
        public void ShowUnknownInstallerFormatMessage(string downloadFileName)
        {
            notify(new Model.NotificationStructure
            {
                Title = "Chyba aktualizace",
                Text = "Při aktualizaci došlo k chybě!",
                NotifyType = NotifyTypes.Warning,
                callback = () =>
                {
                    // TODO: Dodělat nahlášení chyby nebo něco takového
                }
            });
            v("Unknow installer format message -> " + downloadFileName);
        }
    }
}
