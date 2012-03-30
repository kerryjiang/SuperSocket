using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace SuperSocket.SocketBase
{
    public static class Messanger
    {
        private static ConcurrentDictionary<Type, Action<object>> m_EventStore = new ConcurrentDictionary<Type, Action<object>>();

        public static void Register<T>(Action<T> handler)
        {
            m_EventStore.TryAdd(typeof(T), (o) => handler((T)o));
        }

        public static void UnRegister<T>()
        {
            Action<object> handler;
            m_EventStore.TryRemove(typeof(T), out handler);
        }

        public static void Send<T>(T message)
        {
            Action<object> handler;

            if (m_EventStore.TryGetValue(typeof(T), out handler))
                handler.BeginInvoke(message, null, null);
        }

        public static Action<object> GetHandler<T>()
        {
            Action<object> handler;

            if(m_EventStore.TryGetValue(typeof(T), out handler))
                return handler;

            return null;
        }
    }
}
