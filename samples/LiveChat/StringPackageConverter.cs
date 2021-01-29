using System;
using System.Linq;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket;

namespace LiveChat
{
    class StringPackageConverter : IPackageMapper<WebSocketPackage, StringPackageInfo>
    {
        public StringPackageInfo Map(WebSocketPackage package)
        {
            var pack = new StringPackageInfo();
            var arr = package.Message.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            pack.Key = arr[0];
            pack.Body = arr[1];
            return pack;
        }
    }
}