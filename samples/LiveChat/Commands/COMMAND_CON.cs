
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket.Server;

namespace LiveChat
{
    public class CON : IAsyncCommand<ChatSession, StringPackageInfo>
    {
        private RoomService _roomService;

        public CON(RoomService roomService)
        {
            _roomService = roomService;
        }

        public async ValueTask ExecuteAsync(ChatSession session, StringPackageInfo package)
        {
            session.Name = package.Body;
            await _roomService.EnterRoom(session);
        }
    }
}