using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Capsa_Connector.Core.Bases
{
    /// <summary>
    /// Abstract for creating every ViewModel and onchange updating the view
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public virtual void OnViewModelSet()
        {

        }
    }
}
