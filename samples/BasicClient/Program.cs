using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket;
using System.Collections.Specialized;

namespace BasicClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var filter = new MesMessagePipelineFilter();
            var encoder = new MesMessagePackageEncoder();
            var client = new EasyClient<MesMessage, MesMessage>(filter, encoder).AsClient();

            if (!await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 3000)))
            {
                Console.WriteLine("Failed to connect the target server.");
                return;
            }
            //var pack = new RTDEPackage(RTDECommandEnum.Connect, "This is a test");
            //var encoder = new RTDEPackageEncoder();
            //await client.SendAsync(encoder, pack);
            var pack = new MesMessage
            {
                Time = DateTime.Now.ToLongDateString(),
                Command = "Mission",
                SourceId = "MES",
                TargetId = "CAC",
                Transaction = Guid.NewGuid().ToString(),
                CommandParameter = "This is only a test"
            };
            await client.SendAsync<MesMessage>(encoder, pack);

            while (true)
            {
                try
                {
                    var p = await client.ReceiveAsync();

                    if (p == null) // connection dropped
                        break;

                    Console.WriteLine(p.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
