using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    /// <summary>
    /// PolicyRequestFilter
    /// </summary>
    class PolicyRequestFilter : FixedSizeRequestFilter<BinaryRequestInfo>
    {
        private const string m_DefaultRequestInfoKey = "REQU";

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRequestFilter"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public PolicyRequestFilter(int size)
            : base(size)
        {

        }

        /// <summary>
        /// Filters the buffer after the server receive the enough size of data.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left.</param>
        /// <returns></returns>
        protected override BinaryRequestInfo FilterBuffer(IAppSession<BinaryRequestInfo> session, byte[] buffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = length - this.Size;
            byte[] data = new byte[this.Size];
            Buffer.BlockCopy(buffer, offset, data, 0, data.Length);
            return new BinaryRequestInfo(m_DefaultRequestInfoKey, data);
        }
    }
}
