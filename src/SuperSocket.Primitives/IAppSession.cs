using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IAppSession
    {
        string SessionID { get; }

        IChannel Channel { get; }

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        IServerInfo Server { get; }

        event EventHandler Connected;

        event EventHandler Closed;

        object State { get; set; }

        void Initialize(IServerInfo server, IChannel channel);

        object this[object name] { get; set; }
    }
}