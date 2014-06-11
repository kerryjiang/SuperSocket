using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The receive filter which is designed for the protocol whose messages must have a same terminator
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public abstract class TerminatorReceiveFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>, IPackageResolver<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private readonly SearchMarkState<byte> m_SearchState;

        /// <summary>
        /// Gets the state of the search.
        /// </summary>
        /// <value>
        /// The state of the search.
        /// </value>
        protected SearchMarkState<byte> SearchState
        {
            get { return m_SearchState; }
        }

        /// <summary>
        /// The null package info
        /// </summary>
        public readonly static TPackageInfo NullPackageInfo = default(TPackageInfo);

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorReceiveFilter{TPackageInfo}"/> class.
        /// </summary>
        /// <param name="terminator">The terminator.</param>
        protected TerminatorReceiveFilter(byte[] terminator)
        {
            m_SearchState = new SearchMarkState<byte>(terminator);
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
            
            var current = data.Last;

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

        /// <summary>
        /// Resolves the package binary data to package instance
        /// </summary>
        /// <param name="packageData">The package binary data.</param>
        /// <returns>the resolved package instance</returns>
        public abstract TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);

        /// <summary>
        /// Gets/sets the next receive filter which will be used when the next network data is received
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
        /// Resets this filter.
        /// </summary>
        public void Reset()
        {
            m_SearchState.Matched = 0;
            State = FilterState.Normal;
            NextReceiveFilter = null;
        }
    }
}
