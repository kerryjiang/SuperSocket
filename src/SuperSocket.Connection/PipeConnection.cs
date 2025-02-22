using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.InteropServices;

namespace SuperSocket.Connection
{
    public abstract class PipeConnection : PipeConnectionBase
    {
        protected Pipe Input { get; }

        protected Pipe Output { get; }

        public PipeConnection(ConnectionOptions options)
            : this(GetInputPipe(options), GetOutputPipe(options), options)
        {
        }

        public PipeConnection(Pipe input, Pipe output, ConnectionOptions options)
            : base(input.Reader, output.Writer, options)
        {
            Input = input;
            Output = output;
        }

        private static Pipe GetInputPipe(ConnectionOptions connectionOptions)
        {
            return connectionOptions.Input ?? new Pipe();
        }

        private static Pipe GetOutputPipe(ConnectionOptions connectionOptions)
        {
            return connectionOptions.Output ?? new Pipe();
        }

        protected override async Task GetConnectionTask(Task readTask, CancellationToken cancellationToken)
        {
            await Task.WhenAll(FillPipeAsync(Input.Writer, cancellationToken), ProcessSends()).ConfigureAwait(false);
            await base.GetConnectionTask(readTask, cancellationToken).ConfigureAwait(false);
        }
        
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

        protected abstract ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken);

        internal virtual async Task FillPipeAsync(PipeWriter writer, CancellationToken cancellationToken)
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

                    var bytesRead = await FillPipeWithDataAsync(memory, cancellationToken).ConfigureAwait(false);

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
                    await SendOverIOAsync(buffer, CancellationToken.None).ConfigureAwait(false);
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

        protected abstract ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken);

        protected internal ArraySegment<byte> GetArrayByMemory(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray<byte>(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }
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

        protected override void CancelOutputPendingRead()
        {
            this.Output.Reader.CancelPendingRead();
        }
    }
}
