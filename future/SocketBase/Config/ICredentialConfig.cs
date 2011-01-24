using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    public interface ICredentialConfig
    {
        string UserName { get; }

        string Password { get; }
    }
}
