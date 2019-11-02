using System;
using System.Buffers;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace SuperSocket.Channel
{
    internal class DefaultObjectPipe<T> : IObjectPipe<T>, IValueTaskSource<T>, IDisposable
    {
        class BufferSegment
        {
            public T[] Array { get; private set; }

            public BufferSegment Next { get; set; }

            public int Offset { get; set; }

            public int End { get; set; } = -1; // -1 means no item in this segment

            public BufferSegment(T[] array)
            {
                Array = array;
            }

            public bool IsAvailable
            {
                get { return Array.Length > (End + 1); }
            }

            public void Write(T value)
            {
                Array[++End] = value;
            }
        }

        private const int _segmentSize =  5;
        private BufferSegment _first;
        private BufferSegment _current;
        private object _syncRoot = new object();
        private static readonly ArrayPool<T> _pool = ArrayPool<T>.Shared;
        private ManualResetValueTaskSourceCore<T> _taskSourceCore;
        private bool _waiting = false;
        private bool _lastReadIsWait = false;
        private int _length;

        public DefaultObjectPipe()
        {
            SetBufferSegment(CreateSegment());
            _taskSourceCore = new ManualResetValueTaskSourceCore<T>();
        }

        BufferSegment CreateSegment()
        {
            return new BufferSegment(_pool.Rent(_segmentSize));
        }

        private void SetBufferSegment(BufferSegment segment)
        {
            if (_first == null)
                _first = segment;

            var current = _current;

            if (current != null)
                current.Next = segment;

            _current = segment;
        }

        public int Write(T target)
        {
            lock (_syncRoot)
            {
                if (_waiting)
                {
                    _waiting = false;
                    _taskSourceCore.SetResult(target);                    
                    return _length;
                }

                var current = _current;

                if (!current.IsAvailable)
                {
                    current = CreateSegment();
                    SetBufferSegment(current);
                }

                current.Write(target);
                _length++;
                return _length;
            }            
        }

        private bool TryRead(out T value)
        {
            var first = _first;

            if (first.Offset < first.End)
            {
                value = first.Array[first.Offset];
                first.Array[first.Offset] = default;
                first.Offset++;
                return true;
            }
            else if (first.Offset == first.End)
            {
                if (first == _current)
                {
                    value = first.Array[first.Offset];
                    first.Array[first.Offset] = default;
                    first.Offset = 0;
                    first.End = -1;
                    return true;
                }
                else
                {
                    value = first.Array[first.Offset];
                    first.Array[first.Offset] = default;
                    _first = first.Next;
                    _pool.Return(first.Array);
                    return true;
                }
            }

            value = default;
            return false;
        }

        public ValueTask<T> ReadAsync()
        {
            lock (_syncRoot)
            {
                if (TryRead(out T value))
                {
                    if (_lastReadIsWait)
                    {
                        // clear the result saved previously in the taskSource object
                        _taskSourceCore.Reset();
                        _lastReadIsWait = false;
                    }
                    
                    _length--;
                    return new ValueTask<T>(value);
                }                    

                _waiting = true;
                _lastReadIsWait = true;
                _taskSourceCore.Reset();
                return new ValueTask<T>(this, _taskSourceCore.Version);
            }            
        }

        T IValueTaskSource<T>.GetResult(short token)
        {
            return _taskSourceCore.GetResult(token);
        }

        ValueTaskSourceStatus IValueTaskSource<T>.GetStatus(short token)
        {
            return _taskSourceCore.GetStatus(token);
        }

        void IValueTaskSource<T>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _taskSourceCore.OnCompleted(continuation, state, token, flags);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    lock (_syncRoot)
                    {
                        // return all segments into the pool
                        var segment = _first;

                        while (segment != null)
                        {
                            _pool.Return(segment.Array);
                            segment = segment.Next;
                        }

                        _first = null;
                        _current = null;
                    }
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
