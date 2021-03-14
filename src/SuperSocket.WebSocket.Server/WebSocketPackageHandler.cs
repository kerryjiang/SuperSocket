using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.WebSocket.Extensions;
using SuperSocket.WebSocket.Server.Extensions;

namespace SuperSocket.WebSocket.Server
{
    public class WebSocketPackageHandler : IPackageHandler<WebSocketPackage>
    {
        private const string _magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private static Encoding _textEncoding = new UTF8Encoding(false);
        
        private IServiceProvider _serviceProvider;

        private IPackageHandler<WebSocketPackage> _websocketCommandMiddleware;

        private Func<WebSocketSession, WebSocketPackage, ValueTask> _packageHandlerDelegate;

        private Dictionary<string, ISubProtocolHandler> _subProtocolHandlers;

        private ILogger _logger;

        private readonly HandshakeOptions _handshakeOptions;

        private readonly IWebSocketServerMiddleware _websocketServerMiddleware;

        private Dictionary<string, IEnumerable<IWebSocketExtensionFactory>> _extensionFactories;

        private static readonly IPackageEncoder<WebSocketPackage> _defaultMessageEncoder = new WebSocketEncoder();

        public WebSocketPackageHandler(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<HandshakeOptions> handshakeOptions)
        {
            _serviceProvider = serviceProvider;

            _websocketServerMiddleware = serviceProvider.GetService<IWebSocketServerMiddleware>();

            _websocketCommandMiddleware = serviceProvider
                .GetService<IWebSocketCommandMiddleware>() as IPackageHandler<WebSocketPackage>;

            _subProtocolHandlers = serviceProvider.GetServices<ISubProtocolHandler>().ToDictionary(h => h.Name, StringComparer.OrdinalIgnoreCase);
            
            _extensionFactories = serviceProvider.GetServices<IWebSocketExtensionFactory>()
                .GroupBy(f => f.Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            _packageHandlerDelegate = serviceProvider.GetService<Func<WebSocketSession, WebSocketPackage, ValueTask>>();
            _logger = loggerFactory.CreateLogger<WebSocketPackageHandler>();
            _handshakeOptions = handshakeOptions.Value;
        }

        private CloseStatus GetCloseStatusFromPackage(WebSocketPackage package)
        {
            if (package.Data.Length < 2)
            {
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

        public async ValueTask Handle(IAppSession session, WebSocketPackage package)
        {
            var websocketSession = session as WebSocketSession;
            
            if (package.OpCode == OpCode.Handshake)
            {
                websocketSession.HttpHeader = package.HttpHeader;

                // handshake failure
                if (!(await HandleHandshake(websocketSession, package)))
                {
                    websocketSession.CloseWithoutHandshake();
                    return;
                }

                websocketSession.Handshaked = true;
                await _websocketServerMiddleware.HandleSessionHandshakeCompleted(websocketSession);
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

                    try
                    {
                        await websocketSession.SendAsync(package);
                    }
                    catch (InvalidOperationException)
                    {
                         // support the case the client close the connection right after it send the close handshake
                    }
                }
                else
                {
                    websocketSession.CloseWithoutHandshake();
                }
                
                return;
            }
            else if (package.OpCode == OpCode.Ping)
            {
                package.OpCode = OpCode.Pong;
                await websocketSession.SendAsync(package);
                return;
            }
            else if (package.OpCode == OpCode.Pong)
            {
                // don't do anything for Pong for now
                return;
            }

            var protocolHandler = websocketSession.SubProtocolHandler;

            if (protocolHandler != null)
            {
                await protocolHandler.Handle(session, package);
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

        private bool SelectSubProtocol(string requestedProtocols, out string selectedProtocol, out ISubProtocolHandler selectedProtocolHandler)
        {
            var protocols = requestedProtocols.Split(',');

            for (var i = 0; i < protocols.Length; i++)
            {
                protocols[i] = protocols[i].Trim();
            }

            if (_subProtocolHandlers.Any())
            {
                foreach (var proto in protocols)
                {
                    if (_subProtocolHandlers.TryGetValue(proto, out ISubProtocolHandler handler))
                    {
                        selectedProtocol = proto;
                        selectedProtocolHandler = handler;
                        return true;
                    }
                }
            }

            selectedProtocol = string.Empty;
            selectedProtocolHandler = null;
            return false;
        }

        private List<string> SelectExtensions(string requestedExtensions, out List<IWebSocketExtension> extensions)
        {
            extensions = null;

            if (string.IsNullOrEmpty(requestedExtensions) || _extensionFactories.Count == 0)
                return null;

            extensions = new List<IWebSocketExtension>();

            var selectedExtensions = new List<string>();            
            var exts = requestedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var e in exts)
            {
                var line = e.Trim();
                var pos = line.IndexOf(';');
                var extName = pos < 0 ? line : line.Substring(0, pos);

                var options = default(NameValueCollection);

                if (pos >= 0)
                {
                    options = new NameValueCollection();

                    foreach (var pair in line.Substring(pos + 1).Split(';'))
                    {
                        var eqPos = pair.IndexOf('=');
                        
                        if (eqPos < 0)
                        {
                            options.Add(pair, string.Empty);
                            continue;
                        }

                        options.Add(pair.Substring(0, eqPos), pair.Substring(eqPos + 1));
                    }
                }

                if (_extensionFactories.TryGetValue(extName, out IEnumerable<IWebSocketExtensionFactory> extFactories))
                {
                    foreach (var f in extFactories)
                    {
                        var extension = f.Create(options, out var supportedOptions);

                        if (extension == null)
                            continue;

                        if (supportedOptions == null || supportedOptions.Count == 0)
                            line = extension.Name;
                        else
                            line = CreateExtensionResponseItem(extension.Name, supportedOptions);

                        selectedExtensions.Add(line);
                        extensions.Add(extension);
                    }
                }
            }

            return selectedExtensions;
        }

        private string CreateExtensionResponseItem(string name, NameValueCollection options)
        {
            var sb = new StringBuilder();
            sb.Append(name);

            foreach (var key in options.AllKeys)
            {
                var value = options.Get(key);

                sb.Append("; ");

                if (string.IsNullOrEmpty(value))
                {
                    sb.Append(key);
                }
                else
                {
                    sb.Append("; ");
                    sb.Append(key);
                    sb.Append("=");
                    sb.Append(value);
                }
            }

            return sb.ToString();
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

            var handshakeValidator = _handshakeOptions?.HandshakeValidator;

            if (handshakeValidator != null)
            {
                if (!await handshakeValidator(session as WebSocketSession, p))
                    return false;
            }

            var ws = session as WebSocketSession;

            var strProtocols = p.HttpHeader.Items[WebSocketConstant.SecWebSocketProtocol];
            var selectedProtocol = string.Empty;

            if (!string.IsNullOrEmpty(strProtocols))
            {
                if (SelectSubProtocol(strProtocols, out string proto, out ISubProtocolHandler handler))
                {
                    ws.SubProtocol = proto;
                    ws.SubProtocolHandler = handler;
                    selectedProtocol = proto;
                }
            }

            var selectedExtensionHeadItems = SelectExtensions(p.HttpHeader.Items[WebSocketConstant.SecWebSocketExtensions], out List<IWebSocketExtension> extensions);

            if (selectedExtensionHeadItems != null && selectedExtensionHeadItems.Count > 0)
            {
                var pipeChannel = session.Channel as IPipeChannel;                
                pipeChannel.PipelineFilter.Context = new WebSocketPipelineFilterContext
                {
                    Extensions = extensions
                };

                ws.MessageEncoder = new WebSocketEncoder
                {
                    Extensions = extensions
                };
            }
            else
            {
                ws.MessageEncoder = _defaultMessageEncoder;
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

                if (!string.IsNullOrEmpty(selectedProtocol))
                    writer.Write(string.Format(WebSocketConstant.ResponseProtocolLine, selectedProtocol), encoding);

                WriteExtensions(writer, encoding, selectedExtensionHeadItems);
                
                writer.Write("\r\n", encoding);
                writer.FlushAsync().GetAwaiter().GetResult();
            });

            return true;
        }

        private void WriteExtensions(PipeWriter writer, Encoding encoding, IReadOnlyList<string> selectedExtensionHeadItems)
        {
            if (selectedExtensionHeadItems != null && selectedExtensionHeadItems.Count > 0)
            {
                writer.Write(WebSocketConstant.ResponseExtensionsLinePrefix, encoding);

                for (var i = 0; i < selectedExtensionHeadItems.Count; i++)
                {
                    var ext = selectedExtensionHeadItems[i];

                    if (i % 3 == 0) // first item in the line
                    {
                        if (i != 0) // not first line
                            writer.Write(",\r\n\t", encoding);
                    }
                    else
                    {
                        writer.Write(", ", encoding);
                    }

                    writer.Write(ext, encoding);
                }

                writer.Write("\r\n", encoding);
            }
        }
    }
}
