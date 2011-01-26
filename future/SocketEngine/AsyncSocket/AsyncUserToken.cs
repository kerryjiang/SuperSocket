using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{

    /// <summary>
    /// This class is designed for use as the object to be assigned to the SocketAsyncEventArgs.UserToken property. 
    /// </summary>
    class AsyncUserToken : IAsyncRunner
    {
        Socket m_socket;

        public AsyncUserToken() : this(null) { }

        public AsyncUserToken(Socket socket)
        {
            m_socket = socket;
        }

        public Socket Socket
        {
            get { return m_socket; }
            set { m_socket = value; }
        }

        public SocketContext SocketContext { get; set; }

        public IAsyncSocketSession SocketSession { get; set; }
    }
}
