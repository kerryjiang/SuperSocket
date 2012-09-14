using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Facility.PolicyServer
{
    /// <summary>
    /// Flash policy AppServer
    /// </summary>
    public class FlashPolicyServer : PolicyServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlashPolicyServer"/> class.
        /// </summary>
        public FlashPolicyServer()
        {

        }

        /// <summary>
        /// Setups the policy response.
        /// </summary>
        /// <param name="policyFileData">The policy file data.</param>
        /// <returns></returns>
        protected override byte[] SetupPolicyResponse(byte[] policyFileData)
        {
            byte[] response = new byte[policyFileData.Length + 1];
            Array.Copy(policyFileData, 0, response, 0, policyFileData.Length);
            response[policyFileData.Length] = 0x00;
            return response;
        }
    }
}
