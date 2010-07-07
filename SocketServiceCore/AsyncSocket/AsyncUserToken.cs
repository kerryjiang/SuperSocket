using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SuperSocket.SocketServiceCore
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
            SendBuffer = new byte[0];
            Offset = 0;
        }

        public Socket Socket
        {
            get { return m_socket; }
            set { m_socket = value; }
        }

        public byte[] SendBuffer { get; set; }

        public int Offset { get; set; }

        public SocketContext SocketContext { get; set; }

        public IAsyncSocketSession SocketSession { get; set; }
    }
}
