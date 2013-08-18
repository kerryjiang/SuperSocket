using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;

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

        internal static bool IsServerManagerType(Type serverType)
        {
            var currentType = serverType;

            while (true)
            {
                if (currentType.FullName == m_ServerManagerTypeName)
                    return true;

                if (currentType.BaseType == null)
                    return false;

                currentType = currentType.BaseType;
            }
        }

        public ServerTypeMetadata GetServerTypeMetadata(string typeName)
        {
            Type type = null;

            try
            {
                var metadata = new ServerTypeMetadata();
                type = Type.GetType(typeName, false);
                metadata.StatusInfoMetadata = type.GetStatusInfoMetadata();

                if (IsServerManagerType(type))
                    metadata.IsServerManager = true;

                return metadata;
            }
            catch
            {
                return null;
            }
        }
    }
}
