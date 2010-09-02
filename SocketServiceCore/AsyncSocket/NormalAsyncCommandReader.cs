using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SuperSocket.SocketServiceCore.AsyncSocket
{
    class NormalAsyncCommandReader : AsyncCommandReader
    {
        public NormalAsyncCommandReader()
            : base()
        {

        }

        public NormalAsyncCommandReader(IAsyncCommandReader prevReader)
            : base(prevReader)
        {

        }

        public override SearhMarkResult FindCommand(SocketAsyncEventArgs e, byte[] endMark, out byte[] commandData)
        {
            return FindCommandDirectly(e, e.Offset, endMark, out commandData);
        }
    }
}
