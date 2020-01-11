using System;

namespace SuperSocket.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; private set; }

        public object Key { get; private set; }

        public CommandAttribute(string name = null, object key = null)
        {
            Name = name;
            Key = key;
        }
    }
}
