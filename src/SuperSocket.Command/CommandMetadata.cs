using System;

namespace SuperSocket.Command
{
    /// <summary>
    /// Represents metadata for a command, including its name and key.
    /// </summary>
    public class CommandMetadata
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the key associated with the command.
        /// </summary>
        public object Key { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMetadata"/> class with the specified name and key.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="key">The key associated with the command.</param>
        public CommandMetadata(string name, object key)
        {
            Name = name;
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMetadata"/> class with the specified name.
        /// The key is set to the same value as the name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        public CommandMetadata(string name)
            : this(name, name)
        {
        }
    }
}
