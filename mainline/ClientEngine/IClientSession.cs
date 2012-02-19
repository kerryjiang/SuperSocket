using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace SuperSocket.ClientEngine
{
    public interface IClientSession
    {
        int ReceiveBufferSize { get; set; }

        void Connect();

        void Send(byte[] data, int offset, int length);

        void Send(IList<ArraySegment<byte>> segments);

        void Close();

        event EventHandler Connected;

        event EventHandler Closed;

        event EventHandler<ErrorEventArgs> Error;

        event EventHandler<DataEventArgs> DataReceived;
    }
}
