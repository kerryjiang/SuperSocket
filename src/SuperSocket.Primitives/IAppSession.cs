using System;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface IAppSession
    {
        string SessionID { get; }

        IChannel Channel { get; }

        IServerInfo Server { get; }

        event EventHandler Connected;

        event EventHandler Closed;

        object State { get; set; }
    }
}