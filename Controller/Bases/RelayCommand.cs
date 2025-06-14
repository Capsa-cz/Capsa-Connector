using System;
using System.Windows.Input;

namespace Capsa_Connector.Core.Bases
{
    /// <summary>
    /// Responsible for executing Action on RelayCommand excecution
    /// </summary>
    public class RelayCommand : ICommand
    {
        // Fields
        private readonly Action<object> _executeAction;
        private readonly Predicate<object> _canExecuteAction;

        // Constructor
        public RelayCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }
        public RelayCommand(Action<object> executeAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = null;
        }

        // Events
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Methods
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction == null ? true : _canExecuteAction(parameter);
        }

        public void Execute(object? parameter)
        {
            _executeAction(parameter);
        }

        /*public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }*/
    }
}
