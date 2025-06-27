using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a pipe connection that relays data between input and output pipes.
    /// </summary>
    public class RelayPipeConnection : PipeConnection
    {
        /// <summary>
        /// Rebuilds the connection options with the specified input and output pipes.
        /// </summary>
        /// <param name="options">The original connection options.</param>
        /// <param name="pipeIn">The input pipe.</param>
        /// <param name="pipeOut">The output pipe.</param>
        /// <returns>The updated connection options.</returns>
        private static ConnectionOptions RebuildOptionsWithPipes(ConnectionOptions options, Pipe pipeIn, Pipe pipeOut)
        {
            options.Input = pipeIn;
            options.Output = pipeOut;
            return options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayPipeConnection"/> class with the specified options and pipes.
        /// </summary>
        /// <param name="options">The connection options.</param>
        /// <param name="pipeIn">The input pipe.</param>
        /// <param name="pipeOut">The output pipe.</param>
        public RelayPipeConnection(ConnectionOptions options, Pipe pipeIn, Pipe pipeOut)
            : base(RebuildOptionsWithPipes(options, pipeIn, pipeOut))
        {
        }

        /// <summary>
        /// Closes the connection by completing the input and output writers.
        /// </summary>
        protected override void Close()
        {
            Input.Writer.Complete();
            Output.Writer.Complete();
        }

        /// <summary>
        /// Sends data over the connection asynchronously.
        /// </summary>
        /// <param name="buffer">The data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The total number of bytes sent.</returns>
        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            var writer = OutputWriter;
            var total = 0;

            foreach (var data in buffer)
            {
                var result = await writer.WriteAsync(data, cancellationToken);

                if (result.IsCompleted)
                    total += data.Length;
                else if (result.IsCanceled)
                    break;
            }

            return total;
        }

        /// <inheritdoc/>
        protected override ValueTask<int> FillInputPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}