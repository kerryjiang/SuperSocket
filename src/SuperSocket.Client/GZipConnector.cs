using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Connection;

namespace SuperSocket.Client
{
    /// <summary>
    /// Represents a connector that establishes connections with GZip compression.
    /// </summary>
    public class GZipConnector : ConnectorBase
    {
        private CompressionLevel _compressionLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="GZipConnector"/> class with the specified compression level.
        /// </summary>
        /// <param name="compressionLevel">The compression level to use for GZip compression.</param>
        public GZipConnector(CompressionLevel compressionLevel)
            : base()
        {
            _compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Asynchronously connects to a remote endpoint and wraps the stream with GZip compression.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="state">The connection state object.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous connection operation.</returns>
        protected override ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var stream = state.Stream;

            if (stream == null)
            {
                if (state.Socket != null)
                {
                    stream = new NetworkStream(state.Socket, true);
                }
                else
                {
                    throw new Exception("Stream from previous connector is null.");
                }
            }

            try
            {
                var pipeStream = new ReadWriteDelegateStream(
                    stream,
                    new GZipStream(stream, CompressionMode.Decompress),
                    new GZipStream(stream, CompressionMode.Compress));

                state.Stream = pipeStream;
                return new ValueTask<ConnectState>(state);
            }
            catch (Exception e)
            {
                return new ValueTask<ConnectState>(new ConnectState
                {
                    Result = false,
                    Exception = e
                });
            }
        }
    }
}
