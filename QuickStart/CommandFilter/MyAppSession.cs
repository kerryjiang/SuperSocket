using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;

namespace SuperSocket.QuickStart.CommandFilter
{
    public class MyAppSession : AppSession<MyAppSession>
    {
        public bool IsLoggedIn { internal set; get; }
    }
}
