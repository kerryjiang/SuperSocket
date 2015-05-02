using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Metadata;

namespace SuperSocket.SocketEngine
{
    class TypeValidator : MarshalByRefObject
    {
        private const string m_ServerManagerTypeName = "SuperSocket.ServerManager.ManagementServer";

        public bool ValidateTypeName(string typeName)
        {
            Type type = null;

            try
            {
                type = AssemblyUtil.GetType(typeName, false, true);
            }
            catch
            {

            }

            return type != null;
        }

        public AppServerMetadata GetServerTypeMetadata(string typeName)
        {
            try
            {
                return AppServerMetadata.GetAppServerMetadata(Type.GetType(typeName, false));
            }
            catch
            {
                return null;
            }
        }
    }
}
