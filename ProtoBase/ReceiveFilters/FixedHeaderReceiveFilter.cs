using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// FixedHeaderReceiveFilter,
    /// it is the Receive filter base for the protocol which define fixed length header and the header contains the request body length,
    /// you can implement your own Receive filter for this kind protocol easily by inheriting this class
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public abstract class FixedHeaderReceiveFilter<TPackageInfo> : FixedSizeReceiveFilter<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private bool m_FoundHeader = false;

        /// <summary>
        /// Gets the size of the header.
        /// </summary>
        /// <value>
        /// The size of the header.
        /// </value>
        public int HeaderSize { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedHeaderReceiveFilter&lt;TPackageInfo&gt;"/> class.
        /// </summary>
        /// <param name="headerSize">Size of the header.</param>
        protected FixedHeaderReceiveFilter(int headerSize)
            : base(headerSize)
        {
            HeaderSize = headerSize;
        }

        /// <summary>
        /// Gets the body length from header.
        /// </summary>
        /// <param name="bufferStream">The received buffer stream.</param>
        /// <param name="length">The length of the header.</param>
        /// <returns></returns>
        protected abstract int GetBodyLengthFromHeader(IBufferStream bufferStream, int length);

        /// <summary>
        /// Determines whether this instance [can resolve package] the specified package data.
        /// </summary>
        /// <param name="bufferStream">The received buffer stream.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can resolve package] the specified package data; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanResolvePackage(IBufferStream bufferStream)
        {
            if (m_FoundHeader)
                return true;

            var bodyLength = GetBodyLengthFromHeader(bufferStream, HeaderSize);

            if (bodyLength < 0)
            {
                State = FilterState.Error;
                return false;
            }

            // no body part
            if (bodyLength == 0)
            {
                m_FoundHeader = true;
                return true;
            }

            ResetSize(HeaderSize + bodyLength);
            m_FoundHeader = true;
            return false;
        }

        /// <summary>
        /// Resets this receive filters.
        /// </summary>
        public override void Reset()
        {
            m_FoundHeader = false;
            base.Reset();
        }
    }
}
