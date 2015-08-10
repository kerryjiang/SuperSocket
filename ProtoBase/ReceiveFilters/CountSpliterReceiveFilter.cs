using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// This Receive filter is designed for this kind protocol:
    /// each request has fixed count part which splited by a char(byte)
    /// for instance, request is defined like this "#12122#23343#4545456565#343435446#",
    /// because this request is splited into many parts by 5 '#', we can create a Receive filter by CountSpliterRequestFilter((byte)'#', 5)
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public abstract class CountSpliterReceiveFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>, IPackageResolver<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private int m_SpliterFoundCount;

        private readonly SearchMarkState<byte> m_SpliterSearchState;

        private readonly int m_SpliterCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilter{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        protected CountSpliterReceiveFilter(byte[] spliter, int spliterCount)
        {
            m_SpliterSearchState = new SearchMarkState<byte>(spliter);
            m_SpliterCount = spliterCount;
        }

        /// <summary>
        /// Filters the received data.
        /// </summary>
        /// <param name="data">The received data.</param>
        /// <param name="rest">The length of the rest data after filtering.</param>
        /// <returns>the received packageInfo instance</returns>
        public TPackageInfo Filter(BufferList data, out int rest)
        {
            rest = 0;

            var currentSegment = data.Last;
            var readBuffer = currentSegment.Array;
            var offset = currentSegment.Offset;
            var length = currentSegment.Count;

            int parsedLength, pos;

            while (m_SpliterFoundCount < m_SpliterCount)
            {
                pos = readBuffer.SearchMark(offset, length, m_SpliterSearchState, out parsedLength);

                if(pos < 0)
                    return default(TPackageInfo);

                m_SpliterFoundCount++;
                offset += parsedLength;
                length -= parsedLength;
            }

            //Found enougth spliters
            data.SetLastItemLength(offset - currentSegment.Offset);
            Reset();
            rest = length;

            return ResolvePackage(data);
        }

        /// <summary>
        /// Gets or sets the next receive filter. The next receive filter will be used when the next network data is received.
        /// </summary>
        /// <value>
        /// The next receive filter.
        /// </value>
        public IReceiveFilter<TPackageInfo> NextReceiveFilter { get; protected set; }

        /// <summary>
        /// Gets the state of the current filter.
        /// </summary>
        /// <value>
        /// The filter state.
        /// </value>
        public FilterState State { get; protected set; }

        /// <summary>
        /// Resets this receive filter.
        /// </summary>
        public virtual void Reset()
        {
            m_SpliterFoundCount = 0;
            m_SpliterSearchState.Matched = 0;
        }

        /// <summary>
        /// Resolves the package binary data to package instance
        /// </summary>
        /// <param name="packageData">The package binary data.</param>
        /// <returns></returns>
        public abstract TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);
    }
}
