using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Provides an abstract base class for virtual connections, extending pipe-based connection functionality.
    /// </summary>
    public abstract class VirtualConnection : PipeConnection, IVirtualConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualConnection"/> class with the specified connection options.
        /// </summary>
        /// <param name="options">The connection options.</param>
        public VirtualConnection(ConnectionOptions options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        internal override Task FillInputPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async ValueTask<FlushResult> WriteInputPipeDataAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken)
        {
            return await Input.Writer.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
        }
    }
}