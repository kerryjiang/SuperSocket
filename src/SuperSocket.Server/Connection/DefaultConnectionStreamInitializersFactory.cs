using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Authentication;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;

namespace SuperSocket.Server.Connection
{
    /// <summary>
    /// Factory for creating default connection stream initializers.
    /// </summary>
    public class DefaultConnectionStreamInitializersFactory : IConnectionStreamInitializersFactory
    {
        /// <summary>
        /// Gets the compression level used for the connection streams.
        /// </summary>
        public CompressionLevel CompressionLevel { get; }

        private IEnumerable<IConnectionStreamInitializer> _empty = new IConnectionStreamInitializer[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConnectionStreamInitializersFactory"/> class with no compression.
        /// </summary>
        public DefaultConnectionStreamInitializersFactory()
            : this(CompressionLevel.NoCompression)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConnectionStreamInitializersFactory"/> class with the specified compression level.
        /// </summary>
        /// <param name="compressionLevel">The compression level to use for the connection streams.</param>
        public DefaultConnectionStreamInitializersFactory(CompressionLevel compressionLevel)
        {
            CompressionLevel = compressionLevel;
        }

        /// <summary>
        /// Creates a collection of connection stream initializers based on the specified listen options.
        /// </summary>
        /// <param name="listenOptions">The options for the listener.</param>
        /// <returns>A collection of connection stream initializers.</returns>
        public virtual IEnumerable<IConnectionStreamInitializer> Create(ListenOptions listenOptions)
        {
            var connectionStreamInitializers = new List<IConnectionStreamInitializer>();

            if (listenOptions.AuthenticationOptions != null && listenOptions.AuthenticationOptions.EnabledSslProtocols != SslProtocols.None)
            {
                connectionStreamInitializers.Add(new NetworkStreamInitializer());
                connectionStreamInitializers.Add(new SslStreamInitializer());
            }

            if (CompressionLevel != CompressionLevel.NoCompression)
            {
                if (!connectionStreamInitializers.Any())
                {
                    connectionStreamInitializers.Add(new NetworkStreamInitializer());
                }

                connectionStreamInitializers.Add(new GZipStreamInitializer());
            }

            connectionStreamInitializers.ForEach(initializer => initializer.Setup(listenOptions));

            return connectionStreamInitializers;
        }
    }
}