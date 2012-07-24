using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.Test.Config;

namespace SuperSocket.Test
{
    public class ChildrenConfigTestServer : AppServer<TestSession>
    {
        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, SocketBase.Protocol.ICustomProtocol<SocketBase.Command.StringCommandInfo> protocol)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            var childrenConfig = config.GetChildConfig<ChildConfigCollection>("children");

            ChildConfigGlobalValue = childrenConfig.GlobalValue;

            var sum = 0;
            var pro = 1;

            foreach (var c in childrenConfig.OfType<ChildConfig>())
            {
                sum += c.Value;
                pro *= c.Value;
            }

            ChildConfigValueSum = sum;
            ChildConfigValueMultiplyProduct = pro;

            var childConfig = config.GetChildConfig<ChildConfig>("child");
            ChildConfigValue = childConfig.Value;

            return true;
        }

        public int ChildConfigValue { get; private set; }

        public int ChildConfigValueSum { get; private set; }

        public int ChildConfigValueMultiplyProduct { get; private set; }

        public int ChildConfigGlobalValue { get; private set; }
    }
}
