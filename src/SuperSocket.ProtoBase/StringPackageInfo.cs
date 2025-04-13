using SuperSocket;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Represents a package with a string key, body, and parameters.
    /// </summary>
    public class StringPackageInfo : IKeyedPackageInfo<string>, IStringPackage
    {
        /// <summary>
        /// Gets or sets the key associated with the package.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the body of the package.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the parameters of the package.
        /// </summary>
        public string[] Parameters { get; set; }
    }
}