using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase
{
    public interface IServerContainer
    {
        IEnumerable<IAppServer> GetAllServers();

        IAppServer GetServerByName(string name);

        event EventHandler<PermformanceDataEventArgs> PerformanceDataCollected;

        event EventHandler Loaded;
    }
}
