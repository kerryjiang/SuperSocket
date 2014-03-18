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

        public DefaultPipelineProcessor(IPackageHandler<TPackageInfo> packageHandler, IReceiveFilter<TPackageInfo> receiveFilter, IBufferRecycler bufferRecycler = null)
            : this(packageHandler, receiveFilter, bufferRecycler, 0)
        {

        }

        public DefaultPipelineProcessor(IPackageHandler<TPackageInfo> packageHandler, IReceiveFilter<TPackageInfo> receiveFilter, IBufferRecycler bufferRecycler = null, int maxPackageLength = 0)
        {
            m_PackageHandler = packageHandler;
            m_ReceiveFilter = receiveFilter;
            m_BufferRecycler = bufferRecycler ?? new NullBufferRecycler();
            m_ReceiveCache = new ReceiveCache();
            m_MaxPackageLength = maxPackageLength;
        }

        private void PushResetData(ArraySegment<byte> raw, int rest, object state)
        {
            var segment = new ArraySegment<byte>(raw.Array, raw.Count - rest, rest);
            m_ReceiveCache.Add(segment, state);
        }

        public event EventHandler NewReceiveBufferRequired;

        private void FireNewReceiveBufferRequired()
        {
            var handler = NewReceiveBufferRequired;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public virtual ProcessState Process(ArraySegment<byte> segment, object state)
        {
            m_ReceiveCache.Add(segment, state);

            var rest = 0;

            while (true)
            {
                var packageInfo = m_ReceiveFilter.Filter(m_ReceiveCache, out rest);

                if (m_ReceiveFilter.State == FilterState.Error)
                {
                    m_BufferRecycler.Return(m_ReceiveCache.GetAllCachedItems(), 0, m_ReceiveCache.Count);
                    return ProcessState.Error;
                }

                if (m_MaxPackageLength > 0)
                {
                    var length = m_ReceiveCache.Total - rest;

                    if (length > m_MaxPackageLength)
                    {
                        m_BufferRecycler.Return(m_ReceiveCache.GetAllCachedItems(), 0, m_ReceiveCache.Count);
                        throw new Exception(string.Format("Max package length: {0}, current processed length: {1}", m_MaxPackageLength, length));
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
                    return ProcessState.Pending;
                }

                m_ReceiveFilter.Reset();

                var nextReceiveFilter = m_ReceiveFilter.NextReceiveFilter;

                if (nextReceiveFilter != null)
                    m_ReceiveFilter = nextReceiveFilter;

                m_PackageHandler.Handle(packageInfo);

                if (rest <= 0)
                {
                    m_BufferRecycler.Return(m_ReceiveCache.GetAllCachedItems(), 0, m_ReceiveCache.Count);
                    m_ReceiveCache.Clear();
                    return ProcessState.Found;
                }

                m_BufferRecycler.Return(m_ReceiveCache.GetAllCachedItems(), 0, m_ReceiveCache.Count - 1);
                m_ReceiveCache.Clear();
                PushResetData(segment, rest, state);
            }
        }

        public ReceiveCache Cache
        {
            get { return m_ReceiveCache; }
        }
    }
}
