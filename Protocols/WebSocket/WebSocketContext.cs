using SuperSocket.ProtoBase;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.Common;

namespace SuperSocket.WebSocket
{
    public class WebSocketContext
    {
        private const string c_WebSocketContextKey = "WebSocketContext";

        public HttpHeaderInfo HandshakeRequest { get; private set; }

        public string Origin { get; private set; }

        public string UriScheme { get; private set; }

        public string Host { get; private set; }

        public string Path { get; private set; }

        public IAppSession Session { get; private set; }

        private List<ArraySegment<byte>> m_Fragments;

        internal List<ArraySegment<byte>> Fragments
        {
            get
            {
                return m_Fragments;
            }
        }

        /// <summary>
        /// Gets the op code of the current processing package
        /// </summary>
        /// <value>
        /// The op code.
        /// </value>
        public sbyte OpCode { get; internal set; }

        /// <summary>
        /// Gets the length of the payload.
        /// </summary>
        /// <value>
        /// The length of the payload.
        /// </value>
        public int PayloadLength { get; internal set; }

        internal void AppendFragment(IList<ArraySegment<byte>> fragment)
        {
            if (m_Fragments == null)
                m_Fragments = new List<ArraySegment<byte>>();

            m_Fragments.AddRange(fragment);
        }

        internal WebSocketPackageInfo ResolveLastFragment(IList<ArraySegment<byte>> fragment)
        {
            var fragments = m_Fragments;

            if (fragments != null && fragments.Count > 0)
            {
                fragments.AddRange(fragment);
                return CreateWebSocketPackage(fragments);
            }

            return CreateWebSocketPackage(fragment);
        }

        private WebSocketPackageInfo CreateWebSocketPackage(IList<ArraySegment<byte>> fragements)
        {
            //var session = Session;
            throw new NotImplementedException();
        }

        public WebSocketContext(IAppSession session, HttpHeaderInfo request)
        {
            Session = session;
            HandshakeRequest = request;
        }

        public string GetAvailableSubProtocol()
        {
            return HandshakeRequest.Get(WebSocketConstant.SecWebSocketProtocol);
        }

        public static WebSocketContext Get(IAppSession session)
        {
            object context;

            if (!session.Items.TryGetValue(c_WebSocketContextKey, out context))
                return null;

            return context as WebSocketContext;
        }
    }
}
