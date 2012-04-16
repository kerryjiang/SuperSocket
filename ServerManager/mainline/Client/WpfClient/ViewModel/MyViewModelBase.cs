using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace SuperSocket.Management.Client.ViewModel
{
    public abstract class MyViewModelBase : ViewModelBase
    {
#if SILVERLIGHT
        private static AsyncOperation m_AsyncOper = AsyncOperationManager.CreateOperation(null);

        protected Action CreateAsyncOperation(Action operation)
        {
            return () =>
            {
                m_AsyncOper.Post(x => operation(), null);
            };
        }

        protected Action<T> CreateAsyncOperation<T>(Action<T> operation)
        {
            return (t) =>
                {
                    m_AsyncOper.Post(x => operation((T)x), t);
                };
        }

        protected Action<T1, T2> CreateAsyncOperation<T1, T2>(Action<T1, T2> operation)
        {
            return (t1, t2) =>
            {
                m_AsyncOper.Post(x => 
                    {
                        var args = (Tuple<T1, T2>)x;
                        operation(args.Item1, args.Item2);
                    }, new Tuple<T1, T2>(t1, t2));
            };
        }

        protected Action<T1, T2, T3> CreateAsyncOperation<T1, T2, T3>(Action<T1, T2, T3> operation)
        {
            return (t1, t2, t3) =>
            {
                m_AsyncOper.Post(x =>
                {
                    var args = (Tuple<T1, T2, T3>)x;
                    operation(args.Item1, args.Item2, args.Item3);
                }, new Tuple<T1, T2, T3>(t1, t2, t3));
            };
        }
#endif
    }
}
