using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Config
{
    public interface ICredentialConfig
    {
        string UserName { get; }

        string Password { get; }
    }
}
