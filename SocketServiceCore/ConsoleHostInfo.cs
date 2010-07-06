using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SuperSocket.SocketServiceCore
{
    public class ConsoleHostInfo
    {
        public object ServiceInstance { get; set; }

        public IEnumerable<Type> ServiceContracts { get; set; }
    }
}
