using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.Server;

namespace LiveChat
{
    public class MSG : IAsyncCommand<ChatSession, StringPackageInfo>
    {
        private RoomService _roomService;

        public MSG(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async ValueTask ExecuteAsync(ChatSession session, StringPackageInfo package, CancellationToken cancellationToken)
        {
            await _roomService.BroadcastMessage(session, package.Body);
        }
    }
}