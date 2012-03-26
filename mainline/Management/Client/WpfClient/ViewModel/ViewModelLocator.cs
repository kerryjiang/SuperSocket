using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace SuperSocket.Management.Client.ViewModel
{
    public interface IViewModelFactory
    {
        ViewModelBase CreateViewModel();
    }

    class DefaultViewModelAccessor<TViewModel> : IViewModelFactory
        where TViewModel : ViewModelBase, new()
    {
        public ViewModelBase CreateViewModel()
        {
            return new TViewModel();
        }
    }

    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            RegisterViewModelFactory<MainViewModel>();
        }

        private static void RegisterViewModelFactory<TViewModel>()
            where TViewModel : ViewModelBase, new()
        {
            var type = typeof(TViewModel);
            m_ViewModelSource.Add(type.Name, new DefaultViewModelAccessor<TViewModel>());
        }

        private static Dictionary<string, IViewModelFactory> m_ViewModelSource = new Dictionary<string, IViewModelFactory>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, IViewModelFactory> ViewModelSource
        {
            get { return m_ViewModelSource; }
        }
    }
}
