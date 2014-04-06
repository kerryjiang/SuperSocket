using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public abstract class TerminatorReceiveFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>, IPackageResolver<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private readonly SearchMarkState<byte> m_SearchState;

        protected SearchMarkState<byte> SearchState
        {
            get { return m_SearchState; }
        }

        public readonly static TPackageInfo NullPackageInfo = default(TPackageInfo);

        protected TerminatorReceiveFilter(byte[] terminator)
        {
            m_SearchState = new SearchMarkState<byte>(terminator);
        }

        public TPackageInfo Filter(ReceiveCache data, out int rest)
        {
            rest = 0;
            
            var current = data.Current;

            int prevMatched = m_SearchState.Matched;

            int parsedLength;
            int result = current.Array.SearchMark(current.Offset, current.Count, m_SearchState, out parsedLength);

            if (result < 0) //Not found
            {
                return NullPackageInfo;
            }

            //Found
            data.SetLastItemLength(parsedLength);
            rest = current.Count - parsedLength;

            return ResolvePackage(data);
        }

        public abstract TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);

        public IReceiveFilter<TPackageInfo> NextReceiveFilter { get; protected set; }

        public FilterState State { get; protected set; }

        public void Reset()
        {
            m_SearchState.Matched = 0;
        }
    }
}
