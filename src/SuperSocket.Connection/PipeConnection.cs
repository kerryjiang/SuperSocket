using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a pipe-based connection with input and output capabilities.
    /// </summary>
    public abstract class PipeConnection : PipeConnectionBase
    {
        /// <summary>
        /// Gets the input pipe for the connection.
        /// </summary>
        protected Pipe Input { get; }

        /// <summary>
        /// Gets the output pipe for the connection.
        /// </summary>
        protected Pipe Output { get; }

        private readonly TimeSpan sendTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeConnection"/> class with the specified connection options.
        /// </summary>
        /// <param name="options">The connection options.</param>
        public PipeConnection(ConnectionOptions options)
            : this(GetInputPipe(options), GetOutputPipe(options), options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PipeConnection"/> class with the specified input and output pipes and connection options.
        /// </summary>
        /// <param name="input">The input pipe.</param>
        /// <param name="output">The output pipe.</param>
        /// <param name="options">The connection options.</param>
        public PipeConnection(Pipe input, Pipe output, ConnectionOptions options)
            : base(input.Reader, output.Writer, options)
        {
            Input = input;
            Output = output;
            
            if (options.SendTimeout > 0)
            {
                sendTimeout = TimeSpan.FromMilliseconds(options.SendTimeout);
            }
        }

        private static Pipe GetInputPipe(ConnectionOptions connectionOptions)
        {
            return connectionOptions.Input ?? new Pipe();
        }

        private static Pipe GetOutputPipe(ConnectionOptions connectionOptions)
        {
            return connectionOptions.Output ?? new Pipe();
        }

        /// <inheritdoc/>
        protected override async Task GetConnectionTask(Task readTask, CancellationToken cancellationToken)
        {
            await Task.WhenAll(FillInputPipeAsync(Input.Writer, cancellationToken), ProcessSends()).ConfigureAwait(false);
            await base.GetConnectionTask(readTask, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes send operations for the connection.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async Task ProcessSends()
        {
            var output = Output.Reader;

            while (true)
            {
                var completedOrCancelled = await ProcessOutputRead(output).ConfigureAwait(false);

                if (completedOrCancelled)
                {
                    break;
                }
            }

            output.Complete();
        }

        /// <summary>
        /// Fills the input pipe with data received from the connection asynchronously.
        /// </summary>
        /// <param name="writer">The input pipe writer for incoming data.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        internal virtual async Task FillInputPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
        {
            var options = Options;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;

                    if (bufferSize <= 0)
                        bufferSize = 1024 * 4; //4k

                    var memory = writer.GetMemory(bufferSize);

                    var bytesRead = await FillInputPipeWithDataAsync(memory, cancellationToken).ConfigureAwait(false);

                    if (bytesRead == 0)
                    {
                        if (!CloseReason.HasValue)
                            CloseReason = Connection.CloseReason.RemoteClosing;

                        break;
                    }

                    UpdateLastActiveTime();

                    // Tell the PipeWriter how much was read
                    writer.Advance(bytesRead);
                }
                catch (Exception e)
                {
                    if (!IsIgnorableException(e))
                    {
                        if (!(e is OperationCanceledException))
                            OnError("Exception happened in ReceiveAsync", e);

                        if (!CloseReason.HasValue)
                        {
                            CloseReason = cancellationToken.IsCancellationRequested
                                ? Connection.CloseReason.LocalClosing : Connection.CloseReason.SocketError;
                        }
                    }
                    else if (!CloseReason.HasValue)
                    {
                        CloseReason = Connection.CloseReason.RemoteClosing;
                    }

                    break;
                }

                // Make the data available to the PipeReader
                var result = await writer.FlushAsync().ConfigureAwait(false);

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            await writer.CompleteAsync().ConfigureAwait(false);
            // And don't allow writing data to outgoing pipeline
            await Output.Writer.CompleteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Processes output data read from the pipe.
        /// </summary>
        /// <param name="reader">The pipe reader to read data from.</param>
        /// <returns>A value task that represents the asynchronous operation. The result indicates whether the operation is completed or cancelled.</returns>
        protected async ValueTask<bool> ProcessOutputRead(PipeReader reader)
        {
            var result = await reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);

            if (result.IsCanceled)
            {
                return true;
            }

            var completedOrCancelled = result.IsCompleted || result.IsCanceled;

            var buffer = result.Buffer;
            var end = buffer.End;

            if (!buffer.IsEmpty)
            {
                try
                {
                    var sendTask = SendOverIOAsync(buffer, CancellationToken.None);

                    if (sendTask.IsCompleted)
                    {
                        await sendTask.ConfigureAwait(false);
                    }
                    else
                    {
                        await sendTask.AsTask().WaitAsync(sendTimeout).ConfigureAwait(false);
                    }

                    UpdateLastActiveTime();
                }
                catch (Exception e)
                {
                    // Cancel all the work in the connection if encounter an error during sending
                    await CancelAsync().ConfigureAwait(false);

                    if (!IsIgnorableException(e))
                        OnError("Exception happened in SendAsync", e);

                    return true;
                }
            }

            reader.AdvanceTo(end);
            return completedOrCancelled;
        }

        /// <summary>
        /// Sends data over the connection asynchronously using the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The total number of bytes sent.</returns>
        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an array segment from the specified memory buffer.
        /// </summary>
        /// <param name="memory">The memory buffer to extract the array segment from.</param>
        /// <returns>The array segment representing the memory buffer.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the memory buffer is not backed by an array.</exception>
        protected internal ArraySegment<byte> GetArrayByMemory(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray<byte>(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified exception is ignorable.
        /// </summary>
        /// <param name="e">The exception to check.</param>
        /// <returns><c>true</c> if the exception is ignorable; otherwise, <c>false</c>.</returns>
        protected override bool IsIgnorableException(Exception e)
        {
            if (base.IsIgnorableException(e))
                return true;

            if (e is SocketException se)
            {
                if (se.IsIgnorableSocketException())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Fills the input pipe with data received from the connection asynchronously.
        /// </summary>
        /// <param name="memory">The memory buffer to fill.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The total number of bytes read.</returns>
        protected abstract ValueTask<int> FillInputPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);
    }
}
