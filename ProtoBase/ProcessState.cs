using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public enum ProcessState : byte
    {
        Found,
        Pending,
        Error
    }
}
