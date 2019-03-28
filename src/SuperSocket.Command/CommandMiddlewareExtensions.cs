using System;

namespace SuperSocket.Command
{
    public static class CommandMiddlewareExtensions
    {
        public static void UseCommand(this IServer server)
        {
            server.Use<CommandMiddleware>();
        }
    }
}
