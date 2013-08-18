using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;

namespace SuperSocket.ServerManager.Client.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
#if SILVERLIGHT
        private Dispatcher m_Dispatcher = Deployment.Current.Dispatcher;
#else
        private Dispatcher m_Dispatcher = App.Current.Dispatcher;
#endif

        protected Dispatcher Dispatcher
        {
            get { return m_Dispatcher; }
        }

        public event PropertyChangedEventHandler PropertyChanged;


#if SILVERLIGHT
        private AsyncOperation m_AsyncOper = AsyncOperationManager.CreateOperation(null);

        protected AsyncOperation AsyncOper
        {
            get { return m_AsyncOper; }
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (m_AsyncOper.SynchronizationContext == DispatcherSynchronizationContext.Current)
            {
                InternalRaisePropertyChanged(propertyName);
                return;
            }

            m_AsyncOper.Post((x) => InternalRaisePropertyChanged((string)x), propertyName);
        }

        private void InternalRaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
#else
        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
#endif
    }
}
