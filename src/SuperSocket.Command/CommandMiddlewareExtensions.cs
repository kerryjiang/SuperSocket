using System;

namespace SuperSocket.Command
{
    public static class CommandMiddlewareExtensions
    {
        public static void UseCommand<TKey, TPackageInfo>(this IServer server)
            where TPackageInfo : IKeyedPackageInfo<TKey>
        {
            server.Use<CommandMiddleware<TKey, TPackageInfo>>();
        }
    }
}
