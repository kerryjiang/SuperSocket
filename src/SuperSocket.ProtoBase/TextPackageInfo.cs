namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Represents a package containing text data.
    /// </summary>
    public class TextPackageInfo
    {
        /// <summary>
        /// Gets or sets the text content of the package.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Returns the text content of the package.
        /// </summary>
        /// <returns>The text content of the package.</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
