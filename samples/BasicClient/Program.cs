﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace BasicClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new EasyClient<RTDEPackage>(new RTDEPackageFilter()).AsClient();

            if (!await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4040)))
            {
                Console.WriteLine("Failed to connect the target server.");
                return;
            }
            var pack = new RTDEPackage
            { 
                RoboCmd = RoboCmd.Connect,
                Data = "This is a test" 
            };
            var encoder = new RTDEPackageEncoder();
            client.SendAsync(encoder, pack);
            while (true)
            {
                var p = await client.ReceiveAsync();

                if (p == null) // connection dropped
                    break;
                
                Console.WriteLine(p.Data);
            }
        }
    }
}
