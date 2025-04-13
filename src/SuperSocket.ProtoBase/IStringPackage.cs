using SuperSocket;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Represents a package containing a string body.
    /// </summary>
    public interface IStringPackage
    {
        /// <summary>
        /// Gets or sets the body of the package.
        /// </summary>
        string Body { get; set; }
    }
}