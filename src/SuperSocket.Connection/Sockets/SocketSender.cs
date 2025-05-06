using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.ObjectPool;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a sender for asynchronous socket operations using <see cref="SocketAsyncEventArgs"/>.
    /// </summary>
    public class SocketSender : SocketAsyncEventArgs, IValueTaskSource<int>, IResettable
    {
        private Action<object> _continuation;

        private static readonly Action<object> _continuationCompleted = _ => { };

        private List<ArraySegment<byte>> _bufferList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketSender"/> class.
        /// </summary>
        public SocketSender()
            : base(unsafeSuppressExecutionContextFlow: true)
        {
        }

        /// <summary>
        /// Sends data asynchronously over the specified socket.
        /// </summary>
        /// <param name="socket">The socket to send data over.</param>
        /// <param name="buffer">The data to send.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous send operation.</returns>
        internal ValueTask<int> SendAsync(Socket socket, in ReadOnlySequence<byte> buffer)
        {
            SetBuffer(buffer);

            if (socket.SendAsync(this))
            {
                return new ValueTask<int>(this, 0);
            }

            return SocketError != SocketError.Success
                ? new ValueTask<int>(Task.FromException<int>(new SocketException((int)SocketError)))
                : new ValueTask<int>(BytesTransferred);
        }

        private void SetBuffer(in ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                var segment = GetArrayByMemory(buffer.First);
                SetBuffer(segment.Array, segment.Offset, segment.Count);
            }
            else
            {
                var bufferList = _bufferList;

                if (bufferList == null)
                {
                    _bufferList = bufferList = new List<ArraySegment<byte>>();
                }

                foreach (var piece in buffer)
                {
                    bufferList.Add(GetArrayByMemory(piece));
                }

                BufferList = bufferList;
            }
        }

        /// <summary>
        /// Handles the completion of the asynchronous socket operation.
        /// </summary>
        /// <param name="e">The <see cref="SocketAsyncEventArgs"/> instance containing event data.</param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            var continuation = Interlocked.CompareExchange(ref _continuation, _continuationCompleted, null);

            // Trigger continuation action if it is set.
            if (continuation != null)
            {
                var state = UserToken;
                _continuation = _continuationCompleted;
                ThreadPool.UnsafeQueueUserWorkItem(continuation, state, false);
            }

            UserToken = null;
        }

        /// <summary>
        /// Gets the result of the asynchronous operation.
        /// </summary>
        /// <param name="token">The token associated with the operation.</param>
        /// <returns>The number of bytes transferred.</returns>
        public int GetResult(short token)
        {
            _continuation = null;
            return BytesTransferred;
        }

        /// <summary>
        /// Gets the status of the asynchronous operation.
        /// </summary>
        /// <param name="token">The token associated with the operation.</param>
        /// <returns>The status of the operation.</returns>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (!ReferenceEquals(_continuation, _continuationCompleted))
                return ValueTaskSourceStatus.Pending;

            return SocketError == SocketError.Success
                ? ValueTaskSourceStatus.Succeeded
                : ValueTaskSourceStatus.Faulted;
        }

        /// <summary>
        /// Schedules the continuation action for the asynchronous operation.
        /// </summary>
        /// <param name="continuation">The continuation action to invoke.</param>
        /// <param name="state">The state to pass to the continuation action.</param>
        /// <param name="token">The token associated with the operation.</param>
        /// <param name="flags">Flags that control the behavior of the continuation.</param>
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            UserToken = state;

            var prevContinuation = Interlocked.CompareExchange(ref _continuation, continuation, null);

            // The task has already completed, so trigger continuation immediately
            if (ReferenceEquals(prevContinuation, _continuationCompleted))
            {
                UserToken = null;
                ThreadPool.UnsafeQueueUserWorkItem(continuation, state, preferLocal: true);
            }
        }

        /// <summary>
        /// Attempts to reset the state of the sender.
        /// </summary>
        /// <returns><c>true</c> if the state was successfully reset; otherwise, <c>false</c>.</returns>
        public bool TryReset()
        {
            if (BufferList != null)
            {
                BufferList = null;
                _bufferList?.Clear();
            }
            else
            {
                SetBuffer(null, 0, 0);
            }

            return true;
        }

        private ArraySegment<byte> GetArrayByMemory(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray<byte>(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }
    }
}