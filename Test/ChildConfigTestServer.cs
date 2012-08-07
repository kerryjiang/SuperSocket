using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.Test.Config;

namespace SuperSocket.Test
{
    public class ChildConfigTestServer : AppServer<TestSession>
    {
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            var childConfig = config.GetChildConfig<ChildConfig>("child");
            ChildConfigValue = childConfig.Value;
            return true;
        }

        public static int ChildConfigValue { get; private set; }
    }

    public class ChildrenConfigTestServer : AppServer<TestSession>
    {
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
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

            return true;
        }

        public static int ChildConfigValueSum { get; private set; }

        public static int ChildConfigValueMultiplyProduct { get; private set; }

        public static int ChildConfigGlobalValue { get; private set; }
    }
}
