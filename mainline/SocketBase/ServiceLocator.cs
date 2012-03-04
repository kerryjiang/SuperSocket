using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> m_ServiceDict = new Dictionary<Type, object>();

        public static void RegisterService<T>(T serviceInstance)
            where T : class
        {
            m_ServiceDict.Add(typeof(T), serviceInstance);
        }

        public static void RegisterService(object serviceInstance)
        {
            m_ServiceDict.Add(serviceInstance.GetType(), serviceInstance);
        }

        public static T GetService<T>()
            where T : class
        {
            object instance;

            if (m_ServiceDict.TryGetValue(typeof(T), out instance))
                return (T)instance;
            else
                return default(T);
        }
    }
}
