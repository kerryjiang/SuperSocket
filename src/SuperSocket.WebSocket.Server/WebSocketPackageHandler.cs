using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketPackageHandler : IPackageHandler<WebSocketPackage>
    {
        private const string _magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private IServiceProvider _serviceProvider;
        private IPackageHandler<WebSocketPackage> _websocketCommandMiddleware;

        private Func<WebSocketSession, WebSocketPackage, Task> _packageHandlerDelegate;

        public WebSocketPackageHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _websocketCommandMiddleware = serviceProvider
                .GetServices<IMiddleware>()
                .OfType<IWebSocketCommandMiddleware>()
                .FirstOrDefault() as IPackageHandler<WebSocketPackage>;

            _packageHandlerDelegate = serviceProvider.GetService<Func<WebSocketSession, WebSocketPackage, Task>>();
        }

        public async Task Handle(IAppSession session, WebSocketPackage package)
        {
            var websocketSession = session as WebSocketSession;
            
            if (package.OpCode == OpCode.Handshake)
            {
                // handshake failure
                if (!await HandleHandshake(session, package))
                    return;

                websocketSession.Handshaked = true;
                return;
            }


            if (!websocketSession.Handshaked)
            {
                // not pass handshake but receive data package now
                // impossible routine
                return;
            }

            if (package.OpCode == OpCode.Close)
            {
                var message = new WebSocketMessage();

                message.OpCode = OpCode.Close;
                message.Data = package.Data;

                await websocketSession.SendAsync(message);

                //After both sending and receiving a Close message, the server MUST close the underlying TCP connection immediately
                websocketSession.Close();                
                return;
            }

            // application command
            var websocketCommandMiddleware = _websocketCommandMiddleware;

            if (websocketCommandMiddleware != null)
            {
                await websocketCommandMiddleware.Handle(session, package);
                return;
            }

            var packageHandleDelegate = _packageHandlerDelegate;
            
            if (packageHandleDelegate != null)
                await packageHandleDelegate(websocketSession, package);
        }

        private async Task<bool> HandleHandshake(IAppSession session, WebSocketPackage p)
        {
            const string requiredVersion = "13";
            var version = p.HttpHeader.Items[WebSocketConstant.SecWebSocketVersion];

            if (!requiredVersion.Equals(version))
                return false;

            var secWebSocketKey = p.HttpHeader.Items[WebSocketConstant.SecWebSocketKey];

            if (string.IsNullOrEmpty(secWebSocketKey))
            {
                return false;
            }

            var responseBuilder = new StringBuilder();

            string secKeyAccept = string.Empty;

            try
            {
                secKeyAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey + _magic)));
            }
            catch (Exception)
            {
                return false;
            }

            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseHeadLine10);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseUpgradeLine);
            responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseConnectionLine);
            responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseAcceptLine, secKeyAccept);
            responseBuilder.AppendWithCrCf();

            byte[] data = Encoding.UTF8.GetBytes(responseBuilder.ToString());

            await session.SendAsync(data);
            return true;
        }
    }
}
