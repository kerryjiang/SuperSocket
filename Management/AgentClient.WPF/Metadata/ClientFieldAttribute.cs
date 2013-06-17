using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Management.AgentClient.Metadata
{
    public class ClientFieldAttribute : Attribute
    {
        public string Format { get; set; }

        public string Name { get; set; }

        public string PropertyName { get; set; }

        public Type DataType { get; set; }

        public int Order { get; set; }

        public bool OutputInPerfLog { get; set; }

        public string ShortName { get; set; }
    }
}
