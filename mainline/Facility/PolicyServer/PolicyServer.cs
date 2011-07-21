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
    public abstract class PolicyServer : AppServer<PolicySession, BinaryCommandInfo>
    {
        private string m_PolicyFile;
        private string m_PolicyRequest = "<policy-file-request/>";
        protected byte[] PolicyResponse { get; private set; }
        private int m_ExpectedReceivedLength;

        public PolicyServer()
            : base()
        {

        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, ICustomProtocol<BinaryCommandInfo> protocol)
        {
            var policyRequest = config.Options.GetValue("policyRequest");
            if (!string.IsNullOrEmpty(policyRequest))
                m_PolicyRequest = policyRequest;

            m_ExpectedReceivedLength = Encoding.UTF8.GetByteCount(m_PolicyRequest);

            this.Protocol = new FixSizeCommandProtocol(m_ExpectedReceivedLength);

            if (!base.Setup(rootConfig, config, socketServerFactory, protocol))
                return false;

            m_PolicyFile = config.Options.GetValue("policyFile");

            if (string.IsNullOrEmpty(m_PolicyFile))
            {
                Logger.LogError("Configuration option policyFile is required!");
                return false;
            }

            if (!Path.IsPathRooted(m_PolicyFile))
                m_PolicyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, m_PolicyFile);

            if (!File.Exists(m_PolicyFile))
            {
                Logger.LogError("The specified policyFile doesn't exist! " + m_PolicyFile);
                return false;
            }

            PolicyResponse = SetupPolicyResponse(File.ReadAllBytes(m_PolicyFile));
            
            this.CommandHandler += new CommandHandler<PolicySession, BinaryCommandInfo>(PolicyServer_CommandHandler);

            return true;
        }

        protected virtual byte[] SetupPolicyResponse(byte[] policyFileData)
        {
            return policyFileData;
        }

        protected virtual byte[] GetPolicyFileResponse(IPEndPoint clientEndPoint)
        {
            return PolicyResponse;
        }

        void PolicyServer_CommandHandler(PolicySession session, BinaryCommandInfo commandInfo)
        {
            ProcessRequest(session, commandInfo.Data);
        }

        protected virtual void ProcessRequest(PolicySession session, byte[] data)
        {
            var request = Encoding.UTF8.GetString(data);

            if (string.Compare(request, m_PolicyRequest, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                session.Close();
                return;
            }

            session.SendResponse(GetPolicyFileResponse(session.RemoteEndPoint));
        }
    }
}
