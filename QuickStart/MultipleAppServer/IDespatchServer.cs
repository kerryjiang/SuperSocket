using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.QuickStart.MultipleAppServer
{
    interface IDespatchServer
    {
        void DispatchMessage(string sessionKey, string message);
    }
}
