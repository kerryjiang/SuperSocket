using System;
using SuperSocket.Channel;

namespace SuperSocket
{
    public interface ISessionFactory
    {
        IAppSession Create();

        Type SessionType { get; }
    }
}