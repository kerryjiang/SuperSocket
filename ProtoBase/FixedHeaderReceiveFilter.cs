using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public abstract class FixedHeaderReceiveFilter<TPackageInfo> : FixedSizeReceiveFilter<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        private bool m_FoundHeader = false;

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

        protected abstract int GetBodyLengthFromHeader(IList<ArraySegment<byte>> packageData, int length);

        protected override bool CanResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            if (m_FoundHeader)
                return true;

            var bodyLength = GetBodyLengthFromHeader(packageData, HeaderSize);

            if (bodyLength < 0)
                State = FilterState.Error;
            else
                ResetSize(HeaderSize + bodyLength);

            m_FoundHeader = true;
            return false;
        }

        public override void Reset()
        {
            m_FoundHeader = false;
            base.Reset();
        }
    }
}
