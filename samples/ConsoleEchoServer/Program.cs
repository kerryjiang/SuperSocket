using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.Server.Host;
using SuperSocket.Server.Abstractions.Session;
using SuperSocket.Connection;
using System.Buffers;
using System.Text;

namespace ConsoleEchoServer
{
    /// <summary>
    /// A simple line-based protocol filter for text commands.
    /// </summary>
    public class LinePipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
    {
        public LinePipelineFilter() : base(Encoding.UTF8.GetBytes("\r\n"))
        {
        }

        protected override StringPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            var text = buffer.GetString(Encoding.UTF8);
            return new StringPackageInfo { Key = "LINE", Body = text };
        }
    }

    /// <summary>
    /// Application session for handling console-based commands.
    /// </summary>
    public class ConsoleSession : AppSession
    {
        protected override async ValueTask OnSessionConnectedAsync()
        {
            await ((IAppSession)this).SendAsync(Encoding.UTF8.GetBytes("Welcome to Console Echo Server!\r\nType 'quit' to exit, 'help' for commands.\r\n"));
        }

        protected override async ValueTask OnSessionClosedAsync(CloseEventArgs e)
        {
            Console.WriteLine($"Session closed: {e.Reason}");
        }
    }

    /// <summary>
    /// Main program for the console echo server.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Console Echo Server starting...");
            Console.WriteLine("This server reads from stdin and writes to stdout.");
            Console.WriteLine("Type commands and press Enter. Type 'quit' to exit.");
            Console.WriteLine("----------------------------------------");

            var host = SuperSocketHostBuilder.Create<StringPackageInfo, LinePipelineFilter>()
                .UseConsole()
                .UseSession<ConsoleSession>()
                .UsePackageHandler(async (session, package) =>
                {
                    var command = package.Body?.Trim();
                    
                    switch (command?.ToLowerInvariant())
                    {
                        case "quit":
                        case "exit":
                            Console.WriteLine("Server shutting down...");
                            await session.CloseAsync(CloseReason.ApplicationError);
                            Environment.Exit(0);
                            break;
                            
                        case "help":
                            await ((IAppSession)session).SendAsync(Encoding.UTF8.GetBytes(
                                "Available commands:\r\n" +
                                "  help - Show this help message\r\n" +
                                "  time - Show current time\r\n" +
                                "  echo <message> - Echo back the message\r\n" +
                                "  quit/exit - Close the server\r\n"));
                            break;
                            
                        case "time":
                            await ((IAppSession)session).SendAsync(Encoding.UTF8.GetBytes($"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n"));
                            break;
                            
                        case var cmd when cmd?.StartsWith("echo ") == true:
                            var message = command.Substring(5);
                            await ((IAppSession)session).SendAsync(Encoding.UTF8.GetBytes($"Echo: {message}\r\n"));
                            break;
                            
                        case "":
                            // Empty line, do nothing
                            break;
                            
                        default:
                            await ((IAppSession)session).SendAsync(Encoding.UTF8.GetBytes($"Unknown command: '{command}'. Type 'help' for available commands.\r\n"));
                            break;
                    }
                })
                .ConfigureLogging((hostCtx, loggingBuilder) => 
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }
}