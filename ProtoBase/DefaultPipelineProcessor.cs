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

        private ReceiveCache m_ReceiveCache;

        private int m_MaxPackageLength;

        public DefaultPipelineProcessor(IPackageHandler<TPackageInfo> packageHandler, IReceiveFilter<TPackageInfo> receiveFilter)
            : this(packageHandler, receiveFilter, 0)
        {

        }

        public DefaultPipelineProcessor(IPackageHandler<TPackageInfo> packageHandler, IReceiveFilter<TPackageInfo> receiveFilter, int maxPackageLength)
        {
            m_PackageHandler = packageHandler;
            m_ReceiveFilter = receiveFilter;
            m_ReceiveCache = new ReceiveCache();
            m_MaxPackageLength = maxPackageLength;
        }

        private void PushResetData(ArraySegment<byte> raw, int rest)
        {
            var segment = new ArraySegment<byte>(raw.Array, raw.Count - rest, rest);
            m_ReceiveCache.Current = segment;
            m_ReceiveCache.All.Add(segment);
        }

        public event EventHandler NewReceiveBufferRequired;

        private void FireNewReceiveBufferRequired()
        {
            var handler = NewReceiveBufferRequired;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public virtual ProcessState Process(ArraySegment<byte> raw)
        {
            m_ReceiveCache.Current = raw;
            m_ReceiveCache.All.Add(raw);

            var rest = 0;

            while (true)
            {
                var packageInfo = m_ReceiveFilter.Filter(m_ReceiveCache, out rest);

                if (m_ReceiveFilter.State == FilterState.Error)
                    return ProcessState.Error;

                if (m_MaxPackageLength > 0)
                {
                    var length = m_ReceiveCache.Total - rest;

                    if (length > m_MaxPackageLength)
                        throw new Exception(string.Format("Max package length: {0}, current processed length: {1}", m_MaxPackageLength, length));
                }

                //Receive continue
                if (packageInfo == null)
                {
                    if (rest > 0)
                    {
                        PushResetData(raw, rest);
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

                m_ReceiveCache.All.Clear();

                if (rest <= 0)
                {
                    m_ReceiveCache.Current = new ArraySegment<byte>();
                    FireNewReceiveBufferRequired();
                    return ProcessState.Found;
                }

                PushResetData(raw, rest);
            }
        }

        public ReceiveCache Cache
        {
            get { return m_ReceiveCache; }
        }
    }
}
