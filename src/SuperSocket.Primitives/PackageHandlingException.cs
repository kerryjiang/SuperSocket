using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperSocket
{
    /// <summary>
    /// Represents an exception that occurs during the handling of a package.
    /// </summary>
    /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
    public class PackageHandlingException<TPackageInfo> : Exception
    {
        /// <summary>
        /// Gets the package that caused the exception.
        /// </summary>
        public TPackageInfo Package { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageHandlingException{TPackageInfo}"/> class with the specified message, package, and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="package">The package that caused the exception.</param>
        /// <param name="e">The inner exception that caused the current exception.</param>
        public PackageHandlingException(string message, TPackageInfo package, Exception e)
            : base(message, e)
        {
            Package = package;
        }
    }
}