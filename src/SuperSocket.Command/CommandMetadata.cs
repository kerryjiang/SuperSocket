using System;

namespace SuperSocket.Command
{
    public class CommandMetadata
    {
        public string Name { get; private set; }

        public object Key { get; private set; }

        public CommandMetadata(string name, object key)
        {
            Name = name;
            Key = key;
        }

        public CommandMetadata(string name)
            : this(name, name)
        {
            
        }
    }
}
