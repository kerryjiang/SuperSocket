using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.Common;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Facility.PolicyServer
{
    /// <summary>
    /// PolicyServer base class
    /// </summary>
    public abstract class PolicyServer : AppServer<PolicySession, BinaryRequestInfo>
    {
        private string m_PolicyFile;
        private string m_PolicyRequest = "<policy-file-request/>";
        /// <summary>
        /// Gets the policy response.
        /// </summary>
        protected byte[] PolicyResponse { get; private set; }
        private int m_ExpectedReceivedLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyServer"/> class.
        /// </summary>
        public PolicyServer()
        {

        }

        /// <summary>
        /// Setups the specified root config.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
        {
            var policyRequest = config.Options.GetValue("policyRequest");
            if (!string.IsNullOrEmpty(policyRequest))
                m_PolicyRequest = policyRequest;

            m_ExpectedReceivedLength = Encoding.UTF8.GetByteCount(m_PolicyRequest);

            ReceiveFilterFactory = new PolicyReceiveFilterFactory(m_ExpectedReceivedLength);

            m_PolicyFile = config.Options.GetValue("policyFile");

            if (string.IsNullOrEmpty(m_PolicyFile))
            {
                if(Logger.IsErrorEnabled)
                    Logger.Error("Configuration option policyFile is required!");
                return false;
            }

            if (!Path.IsPathRooted(m_PolicyFile))
                m_PolicyFile = GetFilePath(m_PolicyFile);

            if (!File.Exists(m_PolicyFile))
            {
                if (Logger.IsErrorEnabled)
                    Logger.Error("The specified policyFile doesn't exist! " + m_PolicyFile);
                return false;
            }

            PolicyResponse = SetupPolicyResponse(File.ReadAllBytes(m_PolicyFile));

            this.NewRequestReceived += new RequestHandler<PolicySession, BinaryRequestInfo>(PolicyServer_NewRequestReceived);

            return true;
        }

        /// <summary>
        /// Setups the policy response.
        /// </summary>
        /// <param name="policyFileData">The policy file data.</param>
        /// <returns></returns>
        protected virtual byte[] SetupPolicyResponse(byte[] policyFileData)
        {
            return policyFileData;
        }

        /// <summary>
        /// Gets the policy file response.
        /// </summary>
        /// <param name="clientEndPoint">The client end point.</param>
        /// <returns></returns>
        protected virtual byte[] GetPolicyFileResponse(IPEndPoint clientEndPoint)
        {
            return PolicyResponse;
        }

        void PolicyServer_NewRequestReceived(PolicySession session, BinaryRequestInfo requestInfo)
        {
            ProcessRequest(session, requestInfo.Body);
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="data">The data.</param>
        protected virtual void ProcessRequest(PolicySession session, byte[] data)
        {
            var request = Encoding.UTF8.GetString(data);

            if (string.Compare(request, m_PolicyRequest, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                session.Close();
                return;
            }

            var response = GetPolicyFileResponse(session.RemoteEndPoint);
            session.Send(response, 0, response.Length);
        }
    }
}
