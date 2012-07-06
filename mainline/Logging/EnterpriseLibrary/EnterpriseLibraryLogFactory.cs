using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace SuperSocket.Common.Logging.EnterpriseLibrary
{
    public class EnterpriseLibraryLogFactory : LogFactoryBase
    {
        private LogWriter m_LogWriter;

        public EnterpriseLibraryLogFactory()
            : this("logging.config")
        {

        }

        public EnterpriseLibraryLogFactory(string loggingConfig) :
            base(loggingConfig)
        {
            var configurationSource = new FileConfigurationSource(this.ConfigFile);

            var factory = new LogWriterFactory(configurationSource);
            m_LogWriter = factory.Create();
        }

        public override ILog GetLog(string name)
        {
            return new EnterpriseLibraryLog(m_LogWriter, name);
        }
    }
}
