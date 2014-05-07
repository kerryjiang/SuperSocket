using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.SocketBase
{
    public class AppContext
    {
        private const string m_SesionDataSlotName = "Session";

        public static IAppSession CurrentSession
        {
            get
            {
                return Thread.GetData(Thread.GetNamedDataSlot(m_SesionDataSlotName)) as IAppSession;
            }
        }

        private const string m_ServerDataSlotName = "Server";

        internal static LocalDataStoreSlot SetCurrentServer(IAppServer server)
        {
            var slot = Thread.GetNamedDataSlot(m_ServerDataSlotName);
            Thread.SetData(slot, server);
            return slot;
        }

        public static IAppServer CurrentServer
        {
            get
            {
                return Thread.GetData(Thread.GetNamedDataSlot(m_ServerDataSlotName)) as IAppServer;
            }
        }
    }
}
