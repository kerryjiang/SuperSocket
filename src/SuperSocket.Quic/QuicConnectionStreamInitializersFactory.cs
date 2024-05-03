using System.Collections.Generic;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using SuperSocket.Server.Connection;

namespace SuperSocket.Quic;

internal class QuicConnectionStreamInitializersFactory : DefaultConnectionStreamInitializersFactory
{
    public override IEnumerable<IConnectionStreamInitializer> Create(ListenOptions listenOptions)
    {
        var connectionStreamInitializers = new List<IConnectionStreamInitializer>();

        if (CompressionLevel != System.IO.Compression.CompressionLevel.NoCompression)
            connectionStreamInitializers.Add(new GZipStreamInitializer());

        connectionStreamInitializers.Add(new QuicPipeStreamInitializer());
        
        connectionStreamInitializers.ForEach(initializer => initializer.Setup(listenOptions));

        return connectionStreamInitializers;
    }
}