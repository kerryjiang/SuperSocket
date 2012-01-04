using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common.Logging.EnterpriseLibrary
{
    public class EnterpriseLibraryLogFactory : ILogFactory
    {
        public ILog GetLog(string name)
        {
            return new EnterpriseLibraryLog(name);
        }
    }
}
