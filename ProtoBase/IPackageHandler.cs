using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Package handler interface
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public interface IPackageHandler<TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        /// <summary>
        /// Handles the specified received package.
        /// </summary>
        /// <param name="package">The received package.</param>
        void Handle(TPackageInfo package);
    }
}
