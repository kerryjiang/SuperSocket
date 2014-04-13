using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public class DefaultPipelineProcessor<TPackageInfo> : IPipelineProcessor
        where TPackageInfo : IPackageInfo
    {
        private IPackageHandler<TPackageInfo> m_PackageHandler;

        public IReceiveFilter<TPackageInfo> m_ReceiveFilter;

        private IBufferRecycler m_BufferRecycler;

        private ReceiveCache m_ReceiveCache;

        private int m_MaxPackageLength;

        private static readonly IBufferRecycler m_NullBufferRecycler = new NullBufferRecycler();

        public DefaultPipelineProcessor(IPackageHandler<TPackageInfo> packageHandler, IReceiveFilter<TPackageInfo> receiveFilter, IBufferRecycler bufferRecycler = null, int maxPackageLength = 0)
        {
            m_PackageHandler = packageHandler;
            m_ReceiveFilter = receiveFilter;
            m_BufferRecycler = bufferRecycler ?? m_NullBufferRecycler;
            m_ReceiveCache = new ReceiveCache();
            m_MaxPackageLength = maxPackageLength;
        }

        private void PushResetData(ArraySegment<byte> raw, int rest, object state)
        {
            var segment = new ArraySegment<byte>(raw.Array, raw.Offset + raw.Count - rest, rest);
            m_ReceiveCache.Add(segment, state);
        }

        public event EventHandler NewReceiveBufferRequired;

        private void FireNewReceiveBufferRequired()
        {
            var handler = NewReceiveBufferRequired;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public virtual ProcessResult Process(ArraySegment<byte> segment, object state)
        {
            m_ReceiveCache.Add(segment, state);

            var rest = 0;

            while (true)
            {
                var packageInfo = m_ReceiveFilter.Filter(m_ReceiveCache, out rest);

                if (m_ReceiveFilter.State == FilterState.Error)
                {
                    m_BufferRecycler.Return(m_ReceiveCache.GetAllCachedItems(), 0, m_ReceiveCache.Count);
                    return ProcessResult.Create(ProcessState.Error);
                }

                if (m_MaxPackageLength > 0)
                {
                    var length = m_ReceiveCache.Total;

                    if (length > m_MaxPackageLength)
                    {
                        m_BufferRecycler.Return(m_ReceiveCache.GetAllCachedItems(), 0, m_ReceiveCache.Count);
                        return ProcessResult.Create(ProcessState.Error, string.Format("Max package length: {0}, current processed length: {1}", m_MaxPackageLength, length));
                    }
                }

                //Receive continue
                if (packageInfo == null)
                {
                    if (rest > 0)
                    {
                        PushResetData(segment, rest, state);
                        continue;
                    }

                    //Because the current buffer is cached, so new buffer is required for receiving
                    FireNewReceiveBufferRequired();
                    return ProcessResult.Create(ProcessState.Cached);
                }

                m_ReceiveFilter.Reset();

                var nextReceiveFilter = m_ReceiveFilter.NextReceiveFilter;

                if (nextReceiveFilter != null)
                    m_ReceiveFilter = nextReceiveFilter;

                m_PackageHandler.Handle(packageInfo);

                if (packageInfo is IRawPackageInfo)
                {
                    m_ReceiveCache = new ReceiveCache();

                    if (rest <= 0)
                    {
                        return ProcessResult.Create(ProcessState.Cached);
                    }
                }
                else
                {
                    ReturnOtherThanLastBuffer();

                    if (rest <= 0)
                    {
                        return ProcessResult.Create(ProcessState.Completed);
                    }
                }

                PushResetData(segment, rest, state);
            }
        }

        void ReturnOtherThanLastBuffer()
        {
            var bufferList = m_ReceiveCache.GetAllCachedItems();
            var count = bufferList.Count;
            var lastBufferItem = bufferList[count - 1].Key.Array;

            for(var i = count - 2; i >= 0; i--)
            {
                if (bufferList[i].Key.Array != lastBufferItem)
                {
                    m_BufferRecycler.Return(bufferList, 0, i + 1);
                    break;
                }
            }

            m_ReceiveCache.Clear();
        }

        public ReceiveCache Cache
        {
            get { return m_ReceiveCache; }
        }
    }
}
