using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public abstract class CountSpliterReceiveFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>, IPackageResolver<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private int m_SpliterFoundCount;

        private readonly SearchMarkState<byte> m_SpliterSearchState;

        private readonly int m_SpliterCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilter&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        protected CountSpliterReceiveFilter(byte[] spliter, int spliterCount)
        {
            m_SpliterSearchState = new SearchMarkState<byte>(spliter);
            m_SpliterCount = spliterCount;
        }

        public TPackageInfo Filter(ReceiveCache data, out int rest)
        {
            rest = 0;

            var currentSegment = data.Current;
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

        public IReceiveFilter<TPackageInfo> NextReceiveFilter { get; protected set; }

        public FilterState State { get; protected set; }

        public virtual void Reset()
        {
            m_SpliterFoundCount = 0;
            m_SpliterSearchState.Matched = 0;
        }

        public abstract TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);
    }
}
