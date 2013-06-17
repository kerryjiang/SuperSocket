using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Reflection;

namespace SuperSocket.Management.AgentClient.Command
{
    /// <summary>
    /// Provide a command that can bind to ButtonBase.Command 
    /// and accept command parameters for Execute and CanExecute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateCommand<T> : ICommand
    {
        private Func<T, bool> canExecute;
        private Action<T> executeAction;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> executeAction,
            Func<T, bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action<T> executeAction)
        {
            this.executeAction = executeAction;
            this.canExecute = x => true;
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
            if (parameter == null) return true;
            T param = ConvertParameter(parameter);
            return canExecute == null ? true : canExecute(param);
        }

        public void Execute(object parameter)
        {
            T param = ConvertParameter(parameter);
            executeAction(param);
        }

        // Convert parameter to expected type, parsing if necessary
        private T ConvertParameter(object parameter)
        {
            string exceptionMessage = string.Format("Cannot convert {0} to {1}",
                parameter.GetType(), typeof(T));

            T result = default(T);
            if (parameter != null && parameter is T)
            {
                result = (T)parameter;
            }
            else if (parameter is string)
            {
                MethodInfo mi = (from m in typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                 where m.Name == "Parse" && m.GetParameters().Count() == 1
                                 select m).FirstOrDefault();
                if (mi != null)
                {
                    try
                    {
                        result = (T)mi.Invoke(null, new object[] { parameter });
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null) throw ex.InnerException;
                        else throw new InvalidCastException(exceptionMessage);
                    }
                }
            }
            else
            {
                throw new InvalidCastException(exceptionMessage);
            }
            return result;
        }
    }
}
