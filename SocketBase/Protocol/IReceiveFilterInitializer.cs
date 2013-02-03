using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Provide the initializing interface for ReceiveFilter
    /// </summary>
    public interface IReceiveFilterInitializer
    {
        /// <summary>
        /// Initializes the ReceiveFilter with the specified appServer and appSession
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="session">The session.</param>
        void Initialize(IAppServer appServer, IAppSession session);
    }
}
