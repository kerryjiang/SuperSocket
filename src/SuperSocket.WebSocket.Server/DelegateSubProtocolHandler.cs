using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.WebSocket.Server
{
    /// <summary>
    /// Handles WebSocket sub-protocols using delegates.
    /// </summary>
    class DelegateSubProtocolHandler : SubProtocolHandlerBase
    {
        private Func<WebSocketSession, WebSocketPackage, CancellationToken, ValueTask> _packageHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateSubProtocolHandler"/> class with a delegate that does not use a cancellation token.
        /// </summary>
        /// <param name="name">The name of the sub-protocol.</param>
        /// <param name="packageHandler">The delegate to handle the package.</param>
        public DelegateSubProtocolHandler(string name, Func<WebSocketSession, WebSocketPackage, ValueTask> packageHandler)
            : base(name)
        {
            _packageHandler = (session, package, cancellationToken) => packageHandler(session, package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateSubProtocolHandler"/> class with a delegate that uses a cancellation token.
        /// </summary>
        /// <param name="name">The name of the sub-protocol.</param>
        /// <param name="packageHandler">The delegate to handle the package.</param>
        public DelegateSubProtocolHandler(string name, Func<WebSocketSession, WebSocketPackage, CancellationToken, ValueTask> packageHandler)
            : base(name)
        {
            _packageHandler = packageHandler;
        }

        /// <summary>
        /// Handles a WebSocket package for the sub-protocol.
        /// </summary>
        /// <param name="session">The session associated with the package.</param>
        /// <param name="package">The WebSocket package.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        public override async ValueTask Handle(IAppSession session, WebSocketPackage package, CancellationToken cancellationToken)
        {
            await _packageHandler(session as WebSocketSession, package, cancellationToken);
        }
    }
}