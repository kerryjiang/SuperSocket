
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket;

namespace LiveChat
{
    public class RoomService
    {
        private ILogger _logger;

        private HashSet<ChatSession> _users;
        
        public RoomService(ILogger<RoomService> logger)
        {
            _logger = logger;
            _users = new HashSet<ChatSession>();
        }

        public async ValueTask BroadcastMessage(ChatSession session, string message)
        {
            foreach (var u in _users)
            {
                await u.SendAsync($"{session.Name}: {message}");
            }
        }

        public async ValueTask EnterRoom(ChatSession session)
        {
            lock (_users)
            {
                _users.Add(session);
            }

            foreach (var u in _users)
            {
                await u.SendAsync($"{session.Name} entered just now.");
            }

            _logger.LogInformation($"{session.Name} entered.");

            session.Closed += async (s, e) =>
            {
                await LeaveRoom(s as ChatSession);
            };
        }

        public async ValueTask LeaveRoom(ChatSession session)
        {
            lock (_users)
            {
                _users.Remove(session);
            }

            foreach (var u in _users)
            {
                await u.SendAsync($"{session.Name} left.");
            }

            _logger.LogInformation($"{session.Name} left.");
        }
    }
}