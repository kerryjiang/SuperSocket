using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The http receive filter base class
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public abstract class HttpHeaderReceiveFilterBase<TPackageInfo> : TerminatorReceiveFilter<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        /// <summary>
        /// Http header terminator
        /// </summary>
        private static readonly byte[] NewLine = new byte[] { 0x0d, 0x0a, 0x0d, 0x0a };

        /// <summary>
        /// Header part text encoding
        /// </summary>
        public Encoding HeaderEncoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderReceiveFilterBase{TPackageInfo}" /> class.
        /// </summary>
        protected HttpHeaderReceiveFilterBase()
            : this(Encoding.UTF8)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderReceiveFilterBase{TPackageInfo}" /> class.
        /// </summary>
        /// <param name="headerEncoding">Header part text encoding</param>
        protected HttpHeaderReceiveFilterBase(Encoding headerEncoding)
            : this(headerEncoding, NewLine)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderReceiveFilterBase{TPackageInfo}" /> class.
        /// </summary>
        /// <param name="headerEncoding">Header part text encoding</param>
        /// <param name="terminator">the terminator of the header part</param>
        protected HttpHeaderReceiveFilterBase(Encoding headerEncoding, byte[] terminator)
            : base(terminator)
        {
            if (headerEncoding == null)
                throw new ArgumentNullException("headerEncoding");

            HeaderEncoding = headerEncoding;
        }

        /// <summary>
        /// Gets the receive filter for body.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="headerSize">Size of the header.</param>
        /// <returns></returns>
        protected abstract IReceiveFilter<TPackageInfo> GetBodyReceiveFilter(HttpHeaderInfo header, int headerSize);


        /// <summary>
        /// Resolves the HTTP package without body.
        /// </summary>
        /// <param name="header">The http header.</param>
        /// <returns></returns>
        protected abstract TPackageInfo ResolveHttpPackageWithoutBody(HttpHeaderInfo header);

        /// <summary>
        /// Resolves the package binary data to package instance
        /// </summary>
        /// <param name="bufferStream">The received buffer stream.</param>
        /// <returns>the resolved package instance</returns>
        public override TPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            var length = (int)bufferStream.Length;

            string headerData = bufferStream.ReadString(length, HeaderEncoding);

            var header = new HttpHeaderInfo();
            MimeHeaderHelper.ParseHttpHeader(headerData, header);

            var nextReceiveFilter = GetBodyReceiveFilter(header, length);

            if (nextReceiveFilter != null)
            {
                NextReceiveFilter = nextReceiveFilter;
                return default(TPackageInfo);
            }

            return ResolveHttpPackageWithoutBody(header);
        }
    }
}
