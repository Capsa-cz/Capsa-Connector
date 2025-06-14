using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetSparkleUpdater.UI.WPF.Helpers;

namespace Capsa_Connector.ViewModel
{
    public class AvailableUpdateViewModel : ChangeNotifier
    {
        public UpdateAvailableResult Result { get; private set; }
        public AppCastItem CurrentItem { get; private set; }

        public event UserRespondedToUpdate UserResponded;

        public AvailableUpdateViewModel(AppCastItem currentItem)
        {
            CurrentItem = currentItem;
            Result = UpdateAvailableResult.None;
        }

        public void OnSkipClicked()
        {
            Result = UpdateAvailableResult.SkipUpdate;
            UserResponded?.Invoke(this, new NetSparkleUpdater.Events.UpdateResponseEventArgs(Result, CurrentItem));
        }

        public void OnInstallClicked()
        {
            Result = UpdateAvailableResult.InstallUpdate;
            UserResponded?.Invoke(this, new NetSparkleUpdater.Events.UpdateResponseEventArgs(Result, CurrentItem));
        }

        public void OnRemindMeLaterClicked()
        {
            Result = UpdateAvailableResult.RemindMeLater;
            UserResponded?.Invoke(this, new NetSparkleUpdater.Events.UpdateResponseEventArgs(Result, CurrentItem));
        }
    }
}
