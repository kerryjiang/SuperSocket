using System;

namespace SuperSocket
{
    public interface IServerInfo
    {
        string Name { get; }

        object DataContext { get; set; }

        int SessionCount { get; }

        IServiceProvider ServiceProvider { get; }

        ServerState State { get; }
    }
}