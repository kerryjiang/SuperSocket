using System;

namespace SuperSocket.Command
{
    /// <summary>
    /// Specifies metadata for a command, including its name and key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the key associated with the command.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
        /// </summary>
        public CommandAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        public CommandAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class with the specified name and key.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="key">The key associated with the command.</param>
        public CommandAttribute(string name, object key)
            : this(name)
        {
            Key = key;
        }
    }
}
