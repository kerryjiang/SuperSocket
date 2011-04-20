using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The basic interface of GenericServer, generic server is a simpler server which only have start and stop behavior.
    /// It can run together with other AppServers.
    /// </summary>
    public interface IGenericServer
    {
        /// <summary>
        /// Initializes the Generic Server.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        bool Initialize(IGenericServerConfig config, ILogger logger);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
