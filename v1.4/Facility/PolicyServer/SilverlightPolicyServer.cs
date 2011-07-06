using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Facility.PolicyServer
{
    public class SilverlightPolicyServer : PolicyServer
    {
        public SilverlightPolicyServer()
            : base()
        {

        }

        protected override void ProcessRequest(PolicySession session, byte[] data)
        {
            base.ProcessRequest(session, data);
            session.Close();
        }
    }
}
