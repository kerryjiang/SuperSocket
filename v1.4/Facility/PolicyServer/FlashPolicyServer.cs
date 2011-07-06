using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Facility.PolicyServer
{
    public class FlashPolicyServer : PolicyServer
    {
        public FlashPolicyServer()
            : base()
        {

        }

        protected override byte[] SetupPolicyResponse(byte[] policyFileData)
        {
            byte[] response = new byte[policyFileData.Length + 1];
            Array.Copy(policyFileData, 0, response, 0, policyFileData.Length);
            response[policyFileData.Length] = 0x00;
            return response;
        }
    }
}
