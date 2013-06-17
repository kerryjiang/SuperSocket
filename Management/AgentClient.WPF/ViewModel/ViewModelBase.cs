using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows;

namespace SuperSocket.Management.AgentClient.ViewModel
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
    }
}
