using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SuperSocket.ServerManager.Client.Command
{
    /// <summary>
    /// Provide a command that can bind to ButtonBase.Command 
    /// without accepting command parameters for Execute and CanExecute.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private Func<bool> canExecute;
        private Action executeAction;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action executeAction,
            Func<bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action executeAction)
        {
            this.executeAction = executeAction;
            this.canExecute = () => true;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute();
        }

        public void Execute(object parameter)
        {
            executeAction();
        }
    }
}
