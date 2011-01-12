using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Configuration;

namespace PerformanceTestConsole
{
    class Program
    {
        private static List<Thread> m_ThreadPool = new List<Thread>();
        private static int m_MaxThreadCount = 1000;
        private static bool m_Stopped = false;
        private static Semaphore m_Semaphore;
        private static long m_TotalRequests = 0;
        private static int m_RequestInterval = 20;
        private static int m_ReadTimeout = 1000;
        private static int m_WriteTimeout = 1000;

        static void Main(string[] args)
        {
            IPEndPoint remoteEndPoint = null;

            try
            {
                m_MaxThreadCount = Convert.ToInt32(ConfigurationManager.AppSettings["concurrentCount"]);
                m_Semaphore = new Semaphore(m_MaxThreadCount, m_MaxThreadCount);
                m_RequestInterval = Convert.ToInt32(ConfigurationManager.AppSettings["requestInterval"]);
                m_ReadTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["readTimeout"]);
                m_WriteTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["writeTimeout"]);
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["targetServer"]), Convert.ToInt32(ConfigurationManager.AppSettings["targetPort"]));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Press any key to start...");
            Console.ReadKey();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            StartTestThreads(remoteEndPoint);

            Console.WriteLine("Performance test has been started...");
            Console.WriteLine("Press key 'Q' to stop...");
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                Console.WriteLine();
                continue;
            }

            Console.WriteLine();
            m_Stopped = true;

            for (int i = 0; i < m_MaxThreadCount; i++)
                m_Semaphore.WaitOne();

            watch.Stop();

            Console.WriteLine("{0} requests per second!", m_TotalRequests / watch.Elapsed.TotalSeconds);
            
            Console.WriteLine("All threads quit!");
            Console.ReadKey();
        }

        static void StartTestThreads(IPEndPoint remoteEndPoint)
        {
            for (int i = 0; i < m_MaxThreadCount; i++)
            {
                m_Semaphore.WaitOne();
                var thread = new Thread(() => RunTest(i, remoteEndPoint, (r, w) => RunEcho(r, w)));
                m_ThreadPool.Add(thread);
                thread.Start();
                Console.WriteLine("Thread {0} has been started!", i);
                Thread.Sleep(10);
            }
        }

        //private static byte[] m_NewLine = Encoding.ASCII.GetBytes(Environment.NewLine);

        //static void RunTestAsync(int execIndex, IPEndPoint remoteEndPoint)
        //{
        //    var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        //    var connect = Observable.FromAsyncPattern<EndPoint>(socket.BeginConnect, socket.EndConnect);
        //    var whenConnected = connect(remoteEndPoint);
        //    whenConnected.Subscribe((u) => Console.WriteLine("Socket {0} connected!", execIndex),
        //        (e) => Console.WriteLine(e.Message + " " + e.StackTrace));

        //    if (!socket.Connected)
        //    {
        //        Console.WriteLine("Socket {0} cannot connect to server!", execIndex);
        //        m_Semaphore.Release();
        //        return;
        //    }
                        
        //    byte[] buffer = new byte[64];
        //    var receive = Observable.FromAsyncPattern<byte[], int, int, SocketFlags, int>(socket.BeginReceive, socket.EndReceive);
        //    var whenReceive = receive(buffer, 0, buffer.Length, SocketFlags.None);
        //    //TryReceiveLine(whenReceive, buffer);
        //}


        //private static bool TryReceiveLine(IObservable<int> whenReceive, byte[] buffer, out string line)
        //{
        //    bool foundNewLine = false;
        //    bool error = false;
        //    MemoryStream receiveStream = new MemoryStream();

        //    while (!foundNewLine && !error)
        //    {
        //        whenReceive.Subscribe((i) =>
        //            {
        //                receiveStream.Write(buffer, 0, i);
        //                receiveStream.Seek(2, SeekOrigin.End);
        //                if (receiveStream.ReadByte() == m_NewLine[0])
        //                    if (receiveStream.ReadByte() == m_NewLine[1])
        //                        foundNewLine = true;
        //            },
        //            (e) =>
        //            {
        //                error = true;
        //                Console.WriteLine(e.Message + " " + e.StackTrace);
        //            });
        //    }

        //    if (error)
        //    {
        //        line = string.Empty;
        //        return false;
        //    }

        //    receiveStream.Seek(0, SeekOrigin.Begin);
        //    StreamReader reader = new StreamReader(receiveStream, Encoding.ASCII, false);
        //    line = reader.ReadToEnd().Trim();
        //    return true;
        //}
        

        static void RunTest(int execIndex, IPEndPoint remoteEndPoint, Func<StreamReader, StreamWriter, bool> processSocket)
        {
            var socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(remoteEndPoint);
                var socketStream = new NetworkStream(socket);
                socketStream.ReadTimeout = m_ReadTimeout;
                socketStream.WriteTimeout = m_WriteTimeout;
                var socketReader = new StreamReader(socketStream, Encoding.ASCII, false);
                var socketWriter = new StreamWriter(socketStream, Encoding.ASCII, 64);

                var welcomeLine = socketReader.ReadLine();

                while (!m_Stopped)
                {
                    if (!processSocket(socketReader, socketWriter))
                        break;

                    Interlocked.Increment(ref m_TotalRequests);
                    Thread.Sleep(m_RequestInterval);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }

                Console.WriteLine("Thread: {0} has exit!", execIndex);
                m_Semaphore.Release();
            }            
        }

        static bool RunEcho(StreamReader reader, StreamWriter writer)
        {
            string param = Guid.NewGuid().ToString();
            string cmd = "ECHO " + param;
            writer.WriteLine(cmd);
            writer.Flush();
            string line = reader.ReadLine();
            if (!param.Equals(line))
            {
                Console.WriteLine("Not macthed line, expected: {0}, actual: {1}", param, line);
                return false;
            }

            return true;
        }
    }
}
