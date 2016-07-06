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
        private IReceiveFilter<TPackageInfo> m_ReceiveFilter;

        private BufferList m_ReceiveCache;

        private int m_MaxPackageLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPipelineProcessor{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="receiveFilter">The initializing receive filter.</param>
        /// <param name="maxPackageLength">The max package size.</param>
        public DefaultPipelineProcessor(IReceiveFilter<TPackageInfo> receiveFilter, int maxPackageLength = 0)
        {
            m_ReceiveFilter = receiveFilter;
            m_ReceiveCache = new BufferList();
            m_MaxPackageLength = maxPackageLength;
        }

        private void PushResetData(ArraySegment<byte> raw, int rest)
        {
            var segment = new ArraySegment<byte>(raw.Array, raw.Offset + raw.Count - rest, rest);
            m_ReceiveCache.Add(segment);
        }

        private IList<IPackageInfo> GetNotNullOne(IList<IPackageInfo> left, IList<IPackageInfo> right)
        {
            if (left != null)
                return left;

            return right;
        }


        /// <summary>
        /// Processes the input segment.
        /// </summary>
        /// <param name="segment">The input segment.</param>
        /// <returns>
        /// the processing result
        /// </returns>
        public virtual ProcessResult Process(ArraySegment<byte> segment)
        {
            var receiveCache = m_ReceiveCache;

            receiveCache.Add(segment);

            var rest = 0;

            var currentReceiveFilter = m_ReceiveFilter;

            SingleItemList<IPackageInfo> singlePackage = null;

            List<IPackageInfo> packageList = null;

            while (true)
            {
                var packageInfo = currentReceiveFilter.Filter(receiveCache, out rest);

                if (currentReceiveFilter.State == FilterState.Error)
                {
                    return ProcessResult.Create(ProcessState.Error);
                }

                if (m_MaxPackageLength > 0)
                {
                    var length = receiveCache.Total;

                    if (length > m_MaxPackageLength)
                    {
                        return ProcessResult.Create(ProcessState.Error, string.Format("Max package length: {0}, current processed length: {1}", m_MaxPackageLength, length));
                    }
                }


                var nextReceiveFilter = currentReceiveFilter.NextReceiveFilter;

                // don't reset the filter if no request is resolved
                if(packageInfo != null)
                    currentReceiveFilter.Reset();

                if (nextReceiveFilter != null)
                {
                    currentReceiveFilter = nextReceiveFilter;
                    m_ReceiveFilter = currentReceiveFilter;
                }                    

                // continue receive
                if (packageInfo == null)
                {
                    if (rest > 0)
                    {
                        if(rest != segment.Count)
                        {
                            PushResetData(segment, rest);
                        }
                        
                        continue;
                    }

                    return ProcessResult.Create(ProcessState.Cached, GetNotNullOne(packageList, singlePackage));
                }

                if (packageList != null)
                {
                    packageList.Add(packageInfo);
                }
                else if (singlePackage == null)
                    singlePackage = new SingleItemList<IPackageInfo>(packageInfo);
                else
                {
                    if (packageList == null)
                        packageList = new List<IPackageInfo>();

                    packageList.Add(singlePackage[0]);
                    packageList.Add(packageInfo);
                    singlePackage = null;
                }

                if (packageInfo is IBufferedPackageInfo // is a buffered package
                        && (packageInfo as IBufferedPackageInfo).Data is BufferList) // and it uses receive buffer directly
                {
                    // so we need to create a new receive buffer container to use
                    m_ReceiveCache = receiveCache = new BufferList();

                    if (rest <= 0)
                    {
                        return ProcessResult.Create(ProcessState.Cached, GetNotNullOne(packageList, singlePackage));
                    }
                }
                else
                {
                    m_ReceiveCache.Clear();

                    if (rest <= 0)
                    {
                        return ProcessResult.Create(ProcessState.Completed, GetNotNullOne(packageList, singlePackage));
                    }
                }

                PushResetData(segment, rest);
            }
        }


        /// <summary>
        /// cleanup the cached the buffer by resolving them into one package at the end of the piple line
        /// </summary>
        /// <returns>return the processing result</returns>
        public ProcessResult CleanUp()
        {
            var currentReceiveFilter = m_ReceiveFilter as ICleanupReceiveFilter<TPackageInfo>;

            if (currentReceiveFilter == null)
                throw new Exception("The current receive filter doesn't support cleanup");

            var receiveCache = m_ReceiveCache;

            var package = currentReceiveFilter.ResolvePackage(receiveCache);

            if (m_ReceiveFilter.State == FilterState.Error)
                return ProcessResult.Create(ProcessState.Error);

            return ProcessResult.Create(ProcessState.Completed, new SingleItemList<IPackageInfo>(package));
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
