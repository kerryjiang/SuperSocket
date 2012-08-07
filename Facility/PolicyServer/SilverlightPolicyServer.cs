using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Facility.PolicyServer
{
    /// <summary>
    /// Silverlight policy AppServer
    /// </summary>
    public class SilverlightPolicyServer : PolicyServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SilverlightPolicyServer"/> class.
        /// </summary>
        public SilverlightPolicyServer()
            : base()
        {

        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="data">The data.</param>
        protected override void ProcessRequest(PolicySession session, byte[] data)
        {
            base.ProcessRequest(session, data);
            session.Close();
        }
    }
}
