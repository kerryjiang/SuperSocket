using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    /// <summary>
    /// PolicyReceiveFilter
    /// </summary>
    class PolicyReceiveFilter : FixedSizeReceiveFilter<BinaryRequestInfo>
    {
        private const string m_DefaultRequestInfoKey = "REQU";

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyReceiveFilter"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public PolicyReceiveFilter(int size)
            : base(size)
        {

        }

        /// <summary>
        /// Filters the buffer after the server receive the enough size of data.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns></returns>
        protected override BinaryRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied)
        {
            byte[] data = new byte[this.Size];
            Buffer.BlockCopy(buffer, offset, data, 0, data.Length);
            return new BinaryRequestInfo(m_DefaultRequestInfoKey, data);
        }
    }
}
