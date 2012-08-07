using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.QuickStart.BroadcastService
{
    public class BroadcastServer : AppServer<BroadcastSession>
    {
        private Dictionary<string, List<string>> broadcastDict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private object broadcastSyncRoot = new object();

        private Dictionary<string, BroadcastSession> broadcastSessionDict = new Dictionary<string, BroadcastSession>(StringComparer.OrdinalIgnoreCase);
        private object syncRoot = new object();

        public BroadcastServer()
        {
            lock (broadcastSyncRoot)
            {
                //It means that device V001 will receive broadcast messages from C001 and C002,
                //device V002  will receive broadcast messages from C002 and C003
                broadcastDict["C001"] = new List<string> { "V001" };
                broadcastDict["C002"] = new List<string> { "V001", "V002" };
                broadcastDict["C003"] = new List<string> { "V002" };
            }
        }

        internal void RegisterNewSession(BroadcastSession session)
        {
            if (string.IsNullOrEmpty(session.DeviceNumber))
                return;

            lock (syncRoot)
            {
                broadcastSessionDict[session.DeviceNumber] = session;
            }
        }

        internal void RemoveOnlineSession(BroadcastSession session)
        {
            if (string.IsNullOrEmpty(session.DeviceNumber))
                return;

            lock (syncRoot)
            {
                broadcastSessionDict.Remove(session.DeviceNumber);
            }
        }

        internal void BroadcastMessage(BroadcastSession session, string message)
        {
            List<string> targetDeviceNumbers;

            lock (broadcastSyncRoot)
            {
                if(!broadcastDict.TryGetValue(session.DeviceNumber, out targetDeviceNumbers))
                    return;
            }

            if (targetDeviceNumbers == null || targetDeviceNumbers.Count <= 0)
                return;

            List<BroadcastSession> sessions = new List<BroadcastSession>();

            lock (syncRoot)
            {
                BroadcastSession s;

                foreach(var key in targetDeviceNumbers)
                {
                    if (broadcastSessionDict.TryGetValue(key, out s))
                        sessions.Add(s);
                }
            }

            this.AsyncRun(() =>
                {
                    sessions.ForEach(s => s.Send(message));
                });
        }

        protected override void OnSessionClosed(BroadcastSession session, CloseReason reason)
        {
            RemoveOnlineSession(session);
            base.OnSessionClosed(session, reason);
        }
    }
}
