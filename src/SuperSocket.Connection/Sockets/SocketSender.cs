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
    public class SocketSender : SocketAsyncEventArgs, IValueTaskSource<int>, IResettable
    {
        private Action<object> _continuation;

        private static readonly Action<object?> _continuationCompleted = _ => { };

        private List<ArraySegment<byte>> _bufferList;

        public SocketSender()
            : base(unsafeSuppressExecutionContextFlow: true)
        {
        }

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

        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            var continuation = _continuation;

            if (continuation != null && Interlocked.CompareExchange(ref _continuation, _continuationCompleted, continuation) == continuation)
            {
                var state = UserToken;
                UserToken = null;

                ThreadPool.UnsafeQueueUserWorkItem(continuation, state, false);
            }
        }

        public int GetResult(short token)
        {
            _continuation = null;
            return BytesTransferred;
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (!ReferenceEquals(_continuation, _continuationCompleted))
                return ValueTaskSourceStatus.Pending;

            return SocketError == SocketError.Success
                ? ValueTaskSourceStatus.Succeeded
                : ValueTaskSourceStatus.Faulted;
        }

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