using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public abstract class BeginEndMarkReceiveFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>, IPackageResolver<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private readonly SearchMarkState<byte> m_BeginSearchState;
        private readonly SearchMarkState<byte> m_EndSearchState;
        private bool m_FoundBegin = false;

        public BeginEndMarkReceiveFilter(byte[] beginMark, byte[] endMark)
        {
            m_BeginSearchState = new SearchMarkState<byte>(beginMark);
            m_EndSearchState = new SearchMarkState<byte>(endMark);
        }

        public abstract TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);

        public virtual TPackageInfo Filter(ReceiveCache data, out int rest)
        {
            rest = 0;

            int searchEndMarkOffset;
            int searchEndMarkLength;

            var currentSegment = data.Current;
            var readBuffer = currentSegment.Array;
            var offset = currentSegment.Offset;
            var length = currentSegment.Count;

            if (!m_FoundBegin)
            {
                int parsedLength;
                int pos = readBuffer.SearchMark(offset, length, m_BeginSearchState, out parsedLength);

                if (pos < 0)
                {
                    //All received data is part of the begin mark
                    if (m_BeginSearchState.Matched > 0 && data.Total == m_BeginSearchState.Matched)
                        return default(TPackageInfo);

                    //Invalid data, contains invalid data before the regular begin mark
                    State = FilterState.Error;
                    return default(TPackageInfo);
                }

                //Found the matched begin mark
                if (pos != offset)//But not at the beginning, contains invalid data before the regular begin mark
                {
                    State = FilterState.Error;
                    return default(TPackageInfo);
                }

                //Found start mark, then search end mark
                m_FoundBegin = true;

                searchEndMarkOffset = offset + parsedLength;

                //Reach end
                if (offset + length <= searchEndMarkOffset)
                    return default(TPackageInfo);

                searchEndMarkLength = offset + length - searchEndMarkOffset;
            }
            else//Already found begin mark
            {
                searchEndMarkOffset = offset;
                searchEndMarkLength = length;
            }

            while (true)
            {
                int parsedLength;
                var endPos = readBuffer.SearchMark(searchEndMarkOffset, searchEndMarkLength, m_EndSearchState, out parsedLength);

                //Haven't found end mark
                if (endPos < 0)
                {
                    return default(TPackageInfo);
                }

                rest = length - parsedLength;

                data.SetLastItemLength(parsedLength);

                var packageInfo = ResolvePackage(data);

                if (!ReferenceEquals(packageInfo, default(TPackageInfo)))
                {
                    Reset();
                    return packageInfo;
                }

                if (rest > 0)
                {
                    searchEndMarkOffset = endPos + m_EndSearchState.Mark.Length;
                    searchEndMarkLength = rest;
                    continue;
                }

                //Not found end mark
                return default(TPackageInfo);
            }
        }

        public IReceiveFilter<TPackageInfo> NextReceiveFilter { get; protected set; }

        public FilterState State { get; protected set; }

        public void Reset()
        {
            m_BeginSearchState.Matched = 0;
            m_EndSearchState.Matched = 0;
            m_FoundBegin = false;
        }
    }
}
