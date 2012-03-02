using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace SuperSocket.Management.Client.ViewModel
{
    public interface IViewModelAccessor
    {
        ViewModelBase GetViewModel();

        void Clear();
    }

    class DefaultViewModelAccessor<TViewModel> : IViewModelAccessor
        where TViewModel : ViewModelBase, new()
    {
        private ViewModelBase m_CurrentViewModel;

        public ViewModelBase GetViewModel()
        {
            if (m_CurrentViewModel == null)
            {
                lock (this)
                {
                    if (m_CurrentViewModel == null)
                    {
                        m_CurrentViewModel = new TViewModel();
                    }
                }
            }

            return m_CurrentViewModel;
        }

        public void Clear()
        {
            if (m_CurrentViewModel != null)
            {
                m_CurrentViewModel.Cleanup();
                m_CurrentViewModel.Dispose();
                m_CurrentViewModel = null;
            }
        }
    }

    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            //RegisterViewModelFactory<HomeViewModel>();
            //RegisterViewModelFactory<MainViewModel>();
            //RegisterViewModelFactory<LoginViewModel>();
        }

        private static void RegisterViewModelFactory<TViewModel>()
            where TViewModel : ViewModelBase, new()
        {
            var type = typeof(TViewModel);
            m_ViewModelSource.Add(type.Name, new DefaultViewModelAccessor<TViewModel>());
        }

        private static Dictionary<string, IViewModelAccessor> m_ViewModelSource = new Dictionary<string, IViewModelAccessor>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, IViewModelAccessor> ViewModelSource
        {
            get { return m_ViewModelSource; }
        }

        public static void Dispose()
        {
            m_ViewModelSource.Values.ToList().ForEach(v => v.Clear());
            m_ViewModelSource.Clear();
        }

        public static TViewModel GetViewModel<TViewModel>()
            where TViewModel : ViewModelBase
        {
            IViewModelAccessor accessor;

            if (!m_ViewModelSource.TryGetValue(typeof(TViewModel).Name, out accessor))
                return default(TViewModel);

            return (TViewModel)accessor.GetViewModel();
        }

        public static void RemoveViewModel<TViewModel>()
        {
            IViewModelAccessor accessor;

            if (m_ViewModelSource.TryGetValue(typeof(TViewModel).Name, out accessor))
                accessor.Clear();
        }
    }
}
