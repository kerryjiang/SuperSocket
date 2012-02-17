using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.Net.Sockets;
using System.Net;

namespace SuperSocket.SocketEngine
{
    delegate void ErrorHandler(ISocketListener listener, Exception e);

    delegate void NewClientAcceptHandler(ISocketListener listener, Socket client);

    /// <summary>
    /// The interface for socket listener
    /// </summary>
    interface ISocketListener
    {
        /// <summary>
        /// Gets the info of listener
        /// </summary>
        ListenerInfo Info { get; }

        /// <summary>
        /// Gets the end point the listener is working on
        /// </summary>
        IPEndPoint EndPoint { get; }

        /// <summary>
        /// Starts to listen
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// Stops listening
        /// </summary>
        void Stop();

        /// <summary>
        /// Occurs when new client accepted.
        /// </summary>
        event NewClientAcceptHandler NewClientAccepted;

        /// <summary>
        /// Occurs when error got.
        /// </summary>
        event ErrorHandler Error;
    }
}
