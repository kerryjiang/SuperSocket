using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The receive filter which is designed for the protocol with begin and end mark within each message
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public abstract class BeginEndMarkReceiveFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>, IPackageResolver<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private readonly SearchMarkState<byte> m_BeginSearchState;
        private readonly SearchMarkState<byte> m_EndSearchState;
        private bool m_FoundBegin = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeginEndMarkReceiveFilter{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="beginMark">The begin mark.</param>
        /// <param name="endMark">The end mark.</param>
        public BeginEndMarkReceiveFilter(byte[] beginMark, byte[] endMark)
        {
            m_BeginSearchState = new SearchMarkState<byte>(beginMark);
            m_EndSearchState = new SearchMarkState<byte>(endMark);
        }

        private bool CheckChanged(byte[] oldMark, byte[] newMark)
        {
            if (oldMark.Length != newMark.Length)
                return true;

            for (var i = 0; i < oldMark.Length; i++)
            {
                if (oldMark[i] != newMark[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Changes the begin mark.
        /// </summary>
        /// <param name="beginMark">The begin mark.</param>
        public void ChangeBeginMark(byte[] beginMark)
        {
            if (!CheckChanged(m_BeginSearchState.Mark, beginMark))
                return;

            m_BeginSearchState.Change(beginMark);
        }

        /// <summary>
        /// Changes the end mark.
        /// </summary>
        /// <param name="endMark">The end mark.</param>
        public void ChangeEndMark(byte[] endMark)
        {
            if (!CheckChanged(m_EndSearchState.Mark, endMark))
                return;

            m_EndSearchState.Change(endMark);
        }

        /// <summary>
        /// Resolves the package binary data to package instance
        /// </summary>
        /// <param name="packageData">The package binary data.</param>
        /// <returns></returns>
        public abstract TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);

        /// <summary>
        /// Filters the received data.
        /// </summary>
        /// <param name="data">The received data.</param>
        /// <param name="rest">The length of the rest data after filtering.</param>
        /// <returns>the received packageInfo instance</returns>
        public virtual TPackageInfo Filter(BufferList data, out int rest)
        {
            rest = 0;

            int searchEndMarkOffset;
            int searchEndMarkLength;

            var currentSegment = data.Last;
            var readBuffer = currentSegment.Array;
            var offset = currentSegment.Offset;
            var length = currentSegment.Count;

            int totalParsed = 0;

            if (!m_FoundBegin)
            {
                int pos = readBuffer.SearchMark(offset, length, m_BeginSearchState, out totalParsed);

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

                searchEndMarkOffset = offset + totalParsed;

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

                totalParsed += parsedLength; //include begin mark if the mark is found in this round receiving
                rest = length - totalParsed;

                data.SetLastItemLength(totalParsed);

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

        /// <summary>
        /// Gets or sets the next receive filter. The next receive filter will be used when the next network data is received.
        /// </summary>
        /// <value>
        /// The next receive filter.
        /// </value>
        public IReceiveFilter<TPackageInfo> NextReceiveFilter { get; protected set; }

        /// <summary>
        /// Gets or sets the filter state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public FilterState State { get; protected set; }

        /// <summary>
        /// Resets this receive filter.
        /// </summary>
        public void Reset()
        {
            m_BeginSearchState.Matched = 0;
            m_EndSearchState.Matched = 0;
            m_FoundBegin = false;
        }
    }
}
