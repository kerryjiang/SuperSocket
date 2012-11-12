using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.HttpBase
{
    /// <summary>
    /// HttpReceiveFilterBase
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class HttpReceiveFilterBase<TRequestInfo> : TerminatorReceiveFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Http header terminator
        /// </summary>
        private static readonly byte[] NewLine = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// indicate whether the header has been parsed
        /// </summary>
        private bool m_HeaderParsed = false;

        /// <summary>
        /// Gets the header items.
        /// </summary>
        protected NameValueCollection HeaderItems { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpReceiveFilterBase&lt;TRequestInfo&gt;"/> class.
        /// </summary>
        protected HttpReceiveFilterBase(IAppSession session)
            : base(session, NewLine)
        {
            
        }

        /// <summary>
        /// Filters the specified session.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest.</param>
        /// <returns></returns>
        public override TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            if (!m_HeaderParsed)
            {
                return base.Filter(readBuffer, offset, length, toBeCopied, out rest);
            }
            else
            {
                return FilterRequestBody(readBuffer, offset, length, toBeCopied, out rest);
            }
        }

        /// <summary>
        /// Filters the request body.
        /// </summary>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="rest">The rest data size.</param>
        /// <returns></returns>
        protected abstract TRequestInfo FilterRequestBody(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest);

        /// <summary>
        /// Resolves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected override TRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length)
        {
            string header = Encoding.UTF8.GetString(data, offset, length);

            var headerItems = new NameValueCollection();
            MimeHeaderHelper.ParseHttpHeader(header, headerItems);
            HeaderItems = headerItems;

            OnHeaderParsed(headerItems);

            return NullRequestInfo;
        }

        /// <summary>
        /// Called when [header parsed].
        /// </summary>
        /// <param name="header">The header.</param>
        protected virtual void OnHeaderParsed(NameValueCollection header)
        {
            
        }

        /// <summary>
        /// Resets this instance to inital state.
        /// </summary>
        public override void Reset()
        {
            m_HeaderParsed = false;
            HeaderItems = null;
            base.Reset();
        }
    }
}
