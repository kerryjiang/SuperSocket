using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using System.Collections.Specialized;

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
        private static readonly byte[] NewLine = Encoding.ASCII.GetBytes("\r\n\r\n");

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHeaderReceiveFilterBase{TPackageInfo}" /> class.
        /// </summary>
        protected HttpHeaderReceiveFilterBase()
            : base(NewLine)
        {
            
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
        /// Resolves the header package.
        /// </summary>
        /// <param name="packageData">The package data.</param>
        /// <returns></returns>
        public override TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            string headerData = Encoding.ASCII.GetString(packageData);

            var header = new HttpHeaderInfo();
            MimeHeaderHelper.ParseHttpHeader(headerData, header);

            var nextReceiveFilter = GetBodyReceiveFilter(header, packageData.Sum(d => d.Count));

            if (nextReceiveFilter != null)
            {
                NextReceiveFilter = nextReceiveFilter;
                return default(TPackageInfo);
            }

            return ResolveHttpPackageWithoutBody(header);
        }
    }
}
