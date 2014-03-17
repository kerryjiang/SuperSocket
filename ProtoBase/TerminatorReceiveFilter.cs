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

    public abstract class TerminatorReceiveFilter : TerminatorReceiveFilter<StringPackageInfo>
    {
        private readonly Encoding m_Encoding;
        private readonly IStringPackageParser<StringPackageInfo> m_PackageParser;

        public TerminatorReceiveFilter(byte[] terminator, Encoding encoding, IStringPackageParser<StringPackageInfo> packageParser)
            : base(terminator)
        {
            m_Encoding = encoding;
            m_PackageParser = packageParser;
        }

        public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            var encoding = m_Encoding;
            var charsBuffer = new char[encoding.GetMaxCharCount(packageData.Sum(x => x.Count))];

            int bytesUsed, charsUsed;
            bool completed;

            var decoder = encoding.GetDecoder();

            var outputOffset = 0;

            foreach (var segment in packageData)
            {
                decoder.Convert(segment.Array, segment.Offset, segment.Count, charsBuffer, outputOffset, charsBuffer.Length - outputOffset, true, out bytesUsed, out charsUsed, out completed);
                outputOffset += charsUsed;
            }

            return m_PackageParser.Parse(new string(charsBuffer, 0, outputOffset));
        }
    }
}
