using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// This Receive filter is designed for this kind protocol:
    /// each request has fixed count part which splited by a char(byte)
    /// for instance, request is defined like this "#12122#23343#4545456565#343435446#",
    /// because this request is splited into many parts by 5 '#', we can create a Receive filter by CountSpliterRequestFilter((byte)'#', 5)
    /// </summary>
    public class CountSpliterReceiveFilter : CountSpliterReceiveFilter<StringRequestInfo>
    {
        private readonly Encoding m_Encoding;

        private readonly int m_KeyIndex;

        private readonly char m_Spliter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilter"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        public CountSpliterReceiveFilter(byte[] spliter, int spliterCount)
            : this(spliter, spliterCount, Encoding.ASCII)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilter"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        /// <param name="encoding">The encoding.</param>
        public CountSpliterReceiveFilter(byte[] spliter, int spliterCount, Encoding encoding)
            : this(spliter, spliterCount, encoding, 0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CountSpliterReceiveFilter"/> class.
        /// </summary>
        /// <param name="spliter">The spliter.</param>
        /// <param name="spliterCount">The spliter count.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="keyIndex">Index of the key.</param>
        public CountSpliterReceiveFilter(byte[] spliter, int spliterCount, Encoding encoding, int keyIndex)
            : base(spliter, spliterCount)
        {
            m_Encoding = encoding;
            m_KeyIndex = keyIndex;
            m_Spliter = (char)spliter[0];
        }

        /// <summary>
        /// Resolves the package.
        /// </summary>
        /// <param name="packageData">The package data.</param>
        /// <returns></returns>
        public override StringRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            //ignore the first and the last spliter
            var total = packageData.Sum(d => d.Count);
            var body = m_Encoding.GetString(packageData, 1, total - 2);
            var array = body.Split(m_Spliter);
            return new StringRequestInfo(array[m_KeyIndex], body, array);
        }
    }
}
