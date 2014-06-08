using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The application context
    /// </summary>
    public class AppContext
    {
        private const string m_SesionDataSlotName = "Session";

        /// <summary>
        /// Gets the current session from thread context.
        /// </summary>
        /// <value>
        /// The current session.
        /// </value>
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

        /// <summary>
        /// Gets the current server from thread context.
        /// </summary>
        /// <value>
        /// The current server.
        /// </value>
        public static IAppServer CurrentServer
        {
            get
            {
                return Thread.GetData(Thread.GetNamedDataSlot(m_ServerDataSlotName)) as IAppServer;
            }
        }
    }
}
