using System;

namespace SuperSocket.Server.Abstractions.Session
{
    public interface ISessionFactory
    {
        IAppSession Create();

        Type SessionType { get; }
    }
}