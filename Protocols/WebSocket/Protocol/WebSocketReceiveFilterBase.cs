using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// WebSocketReceiveFilter basis
    /// </summary>
    public abstract class WebSocketReceiveFilterBase : ReceiveFilterBase<IWebSocketFragment>
    {
        /// <summary>
        /// The length of Sec3Key
        /// </summary>
        protected const int SecKey3Len = 8;

        private readonly IWebSocketSession m_Session;

        internal IWebSocketSession Session
        {
            get { return m_Session; }
        }

        static WebSocketReceiveFilterBase()
        {
            HandshakeRequestInfo = new HandshakeRequest();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketReceiveFilterBase" /> class.
        /// </summary>
        /// <param name="session">The session.</param>
        protected WebSocketReceiveFilterBase(IWebSocketSession session)
        {
            m_Session = session;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketReceiveFilterBase" /> class.
        /// </summary>
        /// <param name="previousReceiveFilter">The previous receive filter.</param>
        protected WebSocketReceiveFilterBase(WebSocketReceiveFilterBase previousReceiveFilter)
            : base(previousReceiveFilter)
        {
            m_Session = previousReceiveFilter.Session;
        }

        /// <summary>
        /// Handshakes the specified protocol processor.
        /// </summary>
        /// <param name="protocolProcessor">The protocol processor.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        protected bool Handshake(IProtocolProcessor protocolProcessor, IWebSocketSession session)
        {
            IReceiveFilter<IWebSocketFragment> dataFrameReader;

            if (!protocolProcessor.Handshake(session, this, out dataFrameReader))
            {
                session.Close(CloseReason.ServerClosing);
                return false;
            }

            //Processor handshake sucessfully, but output datareader is null, so the multiple protocol switch handled the handshake
            //In this case, the handshake is not completed
            if (dataFrameReader == null)
            {
                NextReceiveFilter = this;
                return false;
            }

            NextReceiveFilter = dataFrameReader;
            return true;
        }

        /// <summary>
        /// Gets the handshake request info.
        /// </summary>
        protected static IWebSocketFragment HandshakeRequestInfo { get; private set; }
    }
}
