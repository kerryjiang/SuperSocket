using System;
using System.Buffers;
using System.Collections.Specialized;
using SuperSocket.WebSocket.Extensions;

namespace SuperSocket.WebSocket.Server.Extensions
{

    public interface IWebSocketExtensionFactory
    {
        string Name { get; }

        IWebSocketExtension Create(NameValueCollection options, out NameValueCollection supportedOptions);
    }
}
