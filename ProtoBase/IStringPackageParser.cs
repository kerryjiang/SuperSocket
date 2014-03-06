using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The interface for request info parser 
    /// </summary>
    public interface IStringPackageParser<out TPackageInfo>
        where TPackageInfo : StringPackageInfo
    {
        /// <summary>
        /// Parses the package info from the source string.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        TPackageInfo Parse(string source);
    }
}
