using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Common.Logging;

namespace SuperSocket.Test
{
    [SetUpFixture]
    public class Setup
    {
        [SetUp]
        public void RunBeforeAllTest()
        {
            if (LogFactoryProvider.LogFactory == null)
                LogFactoryProvider.Initialize(new ConsoleLogFactory());
        }
    }
}
