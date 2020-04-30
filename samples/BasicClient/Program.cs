using System;
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
            //await Test1();
            //await Test2();
            await Test3();
        }

        static async Task Test1()
        {
            var client = new EasyClient<MyPackage>(new MyPackageFilter()).AsClient();

            if (!await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6001)))
            {
                Console.WriteLine("Failed to connect the target server.");
                return;
            }

            while (true)
            {
                var p = await client.ReceiveAsync();

                if (p == null) // connection dropped
                    break;

                Console.WriteLine(p?.Body);
            }
        }

        static async Task Test2()
        {
            MyBeginEndMarkPipelineFilter filter = new MyBeginEndMarkPipelineFilter();

            var client = new EasyClient<StringPackageInfo, StringPackageInfo>(filter).AsClient();

            if (!await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6001)))
            {
                Console.WriteLine("Failed to connect the target server.");
                return;
            }

            int i = 0;
            do
            {

                string msg = @"MSH|^~\&|HIS|LIS|ABC|DEF|20200408143100|pv1|ADT^A08|A6B64388-1F8B-4763-BA4D-B4A76BE58DA9|P|2.4|||||CHN
EVN|A08|20200408143100
PID|1|415320^^^&PATID|415320^^^&PATID~1221284^^^&BLH~800033^^^&BRKH^BLH~0^^^&YEXH||CESHI||19900408000000|M||||||||M^已婚||||||17^汉族||||||40^中国
NK1||CESHI|SEL^SEL
PV1|1|I|4011^急诊外科^29^^^^1001^急诊外科病区||||6371^张三||||||1|||||||2001~现金||||||||||||||||||||1||||||||||
";

                byte[] bytes = Encoding.UTF8.GetBytes(msg);

                List<byte> byteLists = new List<byte>();

                byteLists.AddRange(filter.BeginMark.ToArray());
                byteLists.AddRange(bytes);
                byteLists.AddRange(filter.EndMark.ToArray());

                try
                {
                    await client.SendAsync(new ReadOnlyMemory<byte>(byteLists.ToArray()));
                    var pack = await client.ReceiveAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (i++<2);
        }

        static async Task Test3()
        {
            MyBeginEndMarkPipelineFilter filter = new MyBeginEndMarkPipelineFilter();

            var client = new EasyClient<StringPackageInfo>(filter).AsClient();

            if (!await client.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6001)))
            {
                Console.WriteLine("Failed to connect the target server.");
                return;
            }

            client.PackageHandler += Client_PackageHandler;

            int i = 0;
            do
            {

                string msg = @"MSH|^~\&|HIS|LIS|ABC|DEF|20200408143100|pv1|ADT^A08|A6B64388-1F8B-4763-BA4D-B4A76BE58DA9|P|2.4|||||CHN
EVN|A08|20200408143100
PID|1|415320^^^&PATID|415320^^^&PATID~1221284^^^&BLH~800033^^^&BRKH^BLH~0^^^&YEXH||CESHI||19900408000000|M||||||||M^已婚||||||17^汉族||||||40^中国
NK1||CESHI|SEL^SEL
PV1|1|I|4011^急诊外科^29^^^^1001^急诊外科病区||||6371^张三||||||1|||||||2001~现金||||||||||||||||||||1||||||||||
";


                string d1 = Encoding.UTF8.GetString(filter.BeginMark.ToArray());
                string d2 = Encoding.UTF8.GetString(filter.EndMark.ToArray());

                msg = d1 + msg + d2;


                byte[] bytes = Encoding.UTF8.GetBytes(msg);

                try
                {
                    await client.SendAsync(new ReadOnlyMemory<byte>(bytes));

                    client.StartReceive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (i++ < 2);
        }

        private static void Client_PackageHandler(EasyClient<StringPackageInfo> sender, StringPackageInfo package)
        {
            Console.WriteLine($"Server answer:{Environment.NewLine}"+ package.Body);
        }
    }
}
