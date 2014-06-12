using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The default pipeline processor
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public class DefaultPipelineProcessor<TPackageInfo> : IPipelineProcessor
        where TPackageInfo : IPackageInfo
    {
        private IPackageHandler<TPackageInfo> m_PackageHandler;

        private IReceiveFilter<TPackageInfo> m_ReceiveFilter;

        private IBufferRecycler m_BufferRecycler;

        private static readonly IBufferRecycler s_NullBufferRecycler = new NullBufferRecycler();

        private BufferList m_ReceiveCache;

        private int m_MaxPackageLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPipelineProcessor{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="packageHandler">The package handler.</param>
        /// <param name="receiveFilter">The initializing receive filter.</param>
        /// <param name="maxPackageLength">The max package size.</param>
        /// <param name="bufferRecycler">The buffer recycler.</param>
        public DefaultPipelineProcessor(IPackageHandler<TPackageInfo> packageHandler, IReceiveFilter<TPackageInfo> receiveFilter, int maxPackageLength = 0, IBufferRecycler bufferRecycler = null)
        {
            m_PackageHandler = packageHandler;
            m_ReceiveFilter = receiveFilter;
            m_BufferRecycler = bufferRecycler ?? s_NullBufferRecycler;
            m_ReceiveCache = new BufferList();
            m_MaxPackageLength = maxPackageLength;
        }

        private void PushResetData(ArraySegment<byte> raw, int rest, IBufferState state)
        {
            var segment = new ArraySegment<byte>(raw.Array, raw.Offset + raw.Count - rest, rest);
            m_ReceiveCache.Add(segment, state);
        }

        /// <summary>
        /// Occurs when [new receive buffer required].
        /// </summary>
        public event EventHandler NewReceiveBufferRequired;

        private void FireNewReceiveBufferRequired()
        {
            var handler = NewReceiveBufferRequired;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Processes the input segment.
        /// </summary>
        /// <param name="segment">The input segment.</param>
        /// <param name="state">The buffer state.</param>
        /// <returns>
        /// the processing result
        /// </returns>
        public virtual ProcessResult Process(ArraySegment<byte> segment, IBufferState state)
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

                if (packageInfo is IBufferedPackageInfo // is a buffered package
                        && (packageInfo as IBufferedPackageInfo).Data is BufferList) // and it uses receive buffer directly
                {
                    // so we need to create a new receive buffer container to use
                    m_ReceiveCache = new BufferList();

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

            for (var i = count - 2; i >= 0; i--)
            {
                if (bufferList[i].Key.Array != lastBufferItem)
                {
                    m_BufferRecycler.Return(bufferList, 0, i + 1);
                    break;
                }
            }

            m_ReceiveCache.Clear();
        }

        /// <summary>
        /// Gets the received cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        public BufferList Cache
        {
            get { return m_ReceiveCache; }
        }
    }
}
