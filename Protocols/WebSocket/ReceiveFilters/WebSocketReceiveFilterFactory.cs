using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.ReceiveFilters
{
    interface IWebSocketReceiveFilterFactory
    {
        int Version { get; }

        bool TryHandshake(WebSocketContext context, out IWebSocketReceiveFilter filter);
    }

    class DraftHybi00WebSocketReceiveFilterFactory : WebSocketReceiveFilterFactory<DraftHybi00ReceiveFilter>
    {
        public DraftHybi00WebSocketReceiveFilterFactory()
            : base(0)
        {

        }

        public override bool TryHandshake(WebSocketContext context, out IWebSocketReceiveFilter filter)
        {
            filter = null;

            var header = context.HandshakeRequest;

            var secKey1 = header.Get(WebSocketConstant.SecWebSocketKey1);
            var secKey2 = header.Get(WebSocketConstant.SecWebSocketKey2);

            if (!string.IsNullOrEmpty(secKey1) && !string.IsNullOrEmpty(secKey2))
            {
                filter = new DraftHybi00ReceiveFilter();
                return true;
            }

            return false;
        }
    }

    class MultipleProtocolSwitchReceiveFilterFactory : IWebSocketReceiveFilterFactory
    {
        protected int[] m_Versions;

        public MultipleProtocolSwitchReceiveFilterFactory(int[] versions)
        {
            m_Versions = versions;
        }

        public int Version { get { return 0; } }

        public bool TryHandshake(WebSocketContext context, out IWebSocketReceiveFilter filter)
        {
            var responseBuilder = new StringBuilder();

            responseBuilder.AppendWithCrCf("HTTP/1.1 400 Bad Request");
            responseBuilder.AppendWithCrCf("Upgrade: WebSocket");
            responseBuilder.AppendWithCrCf("Connection: Upgrade");
            responseBuilder.AppendWithCrCf("Sec-WebSocket-Version: " + string.Join(", ", m_Versions.Select(i => i.ToString()).ToArray()));
            responseBuilder.AppendWithCrCf();

            var switchResponse = Encoding.UTF8.GetBytes(responseBuilder.ToString());

            filter = new MultipleProtocolSwitchReceiveFilter(switchResponse);

            return true;
        }
    }

    class WebSocketReceiveFilterFactory<T> : IWebSocketReceiveFilterFactory
        where T : IWebSocketReceiveFilter, new()
    {
        protected string m_VersionTag;

        public WebSocketReceiveFilterFactory(int version)
        {
            Version = version;
            m_VersionTag = version.ToString();
        }

        public int Version { get; private set; }

        public virtual bool TryHandshake(WebSocketContext context, out IWebSocketReceiveFilter filter)
        {
            filter = null;

            var secWebSocketVersion = context.HandshakeRequest.Get(WebSocketConstant.SecWebSocketVersion);

            if (secWebSocketVersion == m_VersionTag)
            {
                filter = new T();
                return true;
            }

            return false;
        }
    }

    static class WebSocketReceiveFilterFactoryManager
    {
        private static IWebSocketReceiveFilterFactory[] m_Factories;

        static WebSocketReceiveFilterFactoryManager()
        {
            var factories = new List<IWebSocketReceiveFilterFactory>();

            factories.Add(new WebSocketReceiveFilterFactory<Rfc6455ReceiveFilter>(13));
            factories.Add(new WebSocketReceiveFilterFactory<DraftHybi10ReceiveFilter>(8));
            factories.Add(new DraftHybi00WebSocketReceiveFilterFactory());
            factories.Add(new MultipleProtocolSwitchReceiveFilterFactory(factories.Where(f => f.Version > 0).Select(f => f.Version).ToArray()));

            m_Factories = factories.ToArray();
        }

        internal static IWebSocketReceiveFilter Handshake(WebSocketContext context)
        {
            IWebSocketReceiveFilter filter;

            foreach(var factory in m_Factories)
            {
                if(factory.TryHandshake(context, out filter))
                {
                    filter.Handshake(context);
                    return filter;
                }
            }

            return null;
        }
    }
}
