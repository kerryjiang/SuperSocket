using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketPackageHandler : IPackageHandler<WebSocketPackage>
    {
        private const string _magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private static Encoding _textEncoding = new UTF8Encoding(false);
        
        private IServiceProvider _serviceProvider;

        private IPackageHandler<WebSocketPackage> _websocketCommandMiddleware;

        private Func<WebSocketSession, WebSocketPackage, Task> _packageHandlerDelegate;

        private ISubProtocolSelector _subProtocolSelector;

        private ILogger _logger;

        public WebSocketPackageHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;

            _websocketCommandMiddleware = serviceProvider
                .GetService<IWebSocketCommandMiddleware>() as IPackageHandler<WebSocketPackage>;

            _packageHandlerDelegate = serviceProvider.GetService<Func<WebSocketSession, WebSocketPackage, Task>>();
            _subProtocolSelector = serviceProvider.GetService<ISubProtocolSelector>();
            _logger = loggerFactory.CreateLogger<WebSocketPackageHandler>();
        }

        private CloseStatus GetCloseStatusFromPackage(WebSocketPackage package)
        {
            if (package.Data.Length < 2)
            {
                _logger.LogWarning($"This close handshake (data length: {package.Data.Length}) doesn't include any close reason or reason text, so default it to normal closure.");

                return new CloseStatus
                {
                    Reason = CloseReason.NormalClosure
                };
            }

            var reader = new SequenceReader<byte>(package.Data);

            reader.TryReadBigEndian(out short closeReason);

            var closeStatus = new CloseStatus
            {
                Reason = (CloseReason)closeReason
            };

            if (reader.Remaining > 0)
            {
                closeStatus.ReasonText = package.Data.Slice(2).GetString(Encoding.UTF8);
            }

            return closeStatus;
        }

        public async Task Handle(IAppSession session, WebSocketPackage package)
        {
            var websocketSession = session as WebSocketSession;
            
            if (package.OpCode == OpCode.Handshake)
            {
                websocketSession.HttpHeader = package.HttpHeader;

                // handshake failure
                if (!(await HandleHandshake(websocketSession, package)))
                    return;

                websocketSession.Handshaked = true;

                var subProtocol = package.HttpHeader.Items["Sec-WebSocket-Protocol"];

                if (!string.IsNullOrEmpty(subProtocol))
                {
                    var subProtocols = subProtocol.Split(',');

                    for (var i = 0; i < subProtocols.Length; i++)
                    {
                        subProtocols[i] = subProtocols[i].Trim();
                    }

                    var subProtocolSelector = _subProtocolSelector;

                    if (subProtocolSelector != null)
                    {
                        var subProtocolSelected = await subProtocolSelector.Select(subProtocols, package.HttpHeader);

                        if (!string.IsNullOrEmpty(subProtocolSelected))
                        {
                            websocketSession.SubProtocol = subProtocolSelected;
                        }
                    }
                }

                await (session.Server as WebSocketService).OnSessionHandshakeCompleted(websocketSession);
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
                if (websocketSession.CloseStatus == null)
                {
                    var closeStatus = GetCloseStatusFromPackage(package);

                    websocketSession.CloseStatus = closeStatus;

                    var message = new WebSocketMessage();

                    message.OpCode = OpCode.Close;
                    message.Data = package.Data;

                    try
                    {
                        await websocketSession.SendAsync(message);
                    }
                    catch (InvalidOperationException)
                    {
                         // support the case the client close the connection right after it send the close handshake
                    }
                }  
                else
                {
                    //After both sending and receiving a Close message, the server MUST close the underlying TCP connection immediately
                    websocketSession.CloseWithoutHandshake();
                }

                return;
            }
            else if (package.OpCode == OpCode.Ping)
            {
                var message = new WebSocketMessage();

                message.OpCode = OpCode.Pong;
                message.Data = package.Data;

                await websocketSession.SendAsync(message);
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

        private async ValueTask<bool> HandleHandshake(IAppSession session, WebSocketPackage p)
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

            string secKeyAccept = string.Empty;

            try
            {
                secKeyAccept = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(secWebSocketKey + _magic)));
            }
            catch (Exception)
            {
                return false;
            }

            var encoding = _textEncoding;

            await session.Channel.SendAsync((writer) =>
            {
                writer.Write(WebSocketConstant.ResponseHeadLine10, encoding);
                writer.Write(WebSocketConstant.ResponseUpgradeLine, encoding);
                writer.Write(WebSocketConstant.ResponseConnectionLine, encoding);
                writer.Write(string.Format(WebSocketConstant.ResponseAcceptLine, secKeyAccept), encoding);
                writer.Write("\r\n", encoding);
                writer.FlushAsync().GetAwaiter().GetResult();
            });

            return true;
        }
    }
}
