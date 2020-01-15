using System;

namespace SuperSocket.Command
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; set; }

        public object Key { get; set; }

        public CommandAttribute(string name, object key)
        {
            Name = name;
            Key = key;
        }
    }
}
