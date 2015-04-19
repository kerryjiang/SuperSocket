using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// the attribute to mark which property of ServerConfig support hot update
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HotUpdateAttribute : Attribute
    {

    }
}
