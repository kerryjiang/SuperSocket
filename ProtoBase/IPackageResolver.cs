using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The package resolver interface
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
    public interface IPackageResolver<out TPackageInfo>
        where TPackageInfo : IPackageInfo
    {
        /// <summary>
        /// Resolves the package binary data to package instance
        /// </summary>
        /// <param name="packageData">The package binary data.</param>
        /// <returns>the resolved package instance</returns>
        TPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData);
    }
}
