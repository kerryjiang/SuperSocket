using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketBase;

namespace SuperSocket.SocketEngine
{

    /// <summary>
    /// This class is designed for use as the object to be assigned to the SocketAsyncEventArgs.UserToken property. 
    /// </summary>
    class AsyncUserToken
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

        public IAsyncSocketSession SocketSession { get; set; }
    }
}
