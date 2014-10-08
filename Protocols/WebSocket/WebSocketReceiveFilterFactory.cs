using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket
{
    public class WebSocketReceiveFilterFactory : IReceiveFilterFactory<StringPackageInfo>
    {
        public WebSocketReceiveFilterFactory()
        {
            var appServer = AppContext.CurrentServer;

            appServer.RegisterService<WebSocketServiceProvider>(new WebSocketServiceProvider
                {
                    BinaryDataParser = appServer.GetService<IBinaryDataParser>(),
                    StringParser = appServer.GetService<IStringParser>()
                });
        }
        public IReceiveFilter<StringPackageInfo> CreateFilter(IAppServer appServer, AppSession appSession, IPEndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }
    }
}
