using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common.Logging
{
    public class ConsoleLogFactory : ILogFactory
    {
        public ILog GetLog(string name)
        {
            return new ConsoleLog(name);
        }
    }
}
