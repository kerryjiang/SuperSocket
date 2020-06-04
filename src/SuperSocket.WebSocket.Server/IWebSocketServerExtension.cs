using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    public interface IWebSocketServerExtension : IWebSocketExtension
    {
        bool Select(IEnumerable<string> availableExtensions, out string selectedExtension);
    }
}