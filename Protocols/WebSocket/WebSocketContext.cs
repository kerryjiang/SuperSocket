using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket
{
    public class WebSocketContext
    {
        private const string c_WebSocketContextKey = "WebSocketContext";

        public bool Handshaked { get; set; }

        public HttpHeaderInfo HandshakeRequest { get; internal set; }

        public string Origin { get; private set; }

        public string UriScheme { get; private set; }

        public string Host { get; private set; }

        public string Path { get; private set; }

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
        public OpCode OpCode { get; internal set; }

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
                fragment = fragments;
            }

            throw new NotSupportedException();
        }

        public IBufferManager BufferManager { get; private set; }

        internal ICommunicationChannel Channel { get; private set; }

        public WebSocketContext(ICommunicationChannel channel, IBufferManager bufferManager)
        {
            BufferManager = bufferManager;
            Channel = channel;

            var session = channel as IAppSession;

            if (session != null)
                session.Items.Add(c_WebSocketContextKey, this);
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
