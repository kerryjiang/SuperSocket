using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SuperSocket.Tests
{
    class DerivedNetworkStream : NetworkStream
    {
        public DerivedNetworkStream(Socket socket)
            : this(socket, false)
        {

        }

        public DerivedNetworkStream(Socket socket, bool ownSocket)
            : base(socket, ownSocket)
        {

        }

        public new Socket Socket
        {
            get { return base.Socket; }
        }
    }
}