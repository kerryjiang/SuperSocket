using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// The udp package interface
    /// </summary>
    public interface IUdpPackageInfo
    {
        /// <summary>
        /// Gets the session ID.
        /// </summary>
        /// <value>
        /// The session ID.
        /// </value>
        string SessionID { get; }
    }

    /// <summary>
    /// UdpPackageInfo, it is designed for passing in business session ID to udp package info
    /// </summary>
    public abstract class UdpPackageInfo<TKey> : IPackageInfo<TKey>, IUdpPackageInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpPackageInfo{TKey}" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="sessionID">The session ID.</param>
        public UdpPackageInfo(TKey key, string sessionID)
        {
            Key = key;
            SessionID = sessionID;
        }

        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        public TKey Key { get; private set; }

        /// <summary>
        /// Gets the session ID.
        /// </summary>
        public string SessionID { get; private set; }
    }
}
