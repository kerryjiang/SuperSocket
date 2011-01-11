using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace PerformanceTestConsole
{
    class Program
    {
        private static List<Thread> m_ThreadPool = new List<Thread>();
        private const int m_MaxThreadCount = 500;
        private static bool m_Stopped = false;
        private static Semaphore m_Semaphore = new Semaphore(m_MaxThreadCount, m_MaxThreadCount);

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start...");
            Console.ReadKey();
            StartTestThreads();
            Console.WriteLine("Performance test has been started...");
            Console.WriteLine("Press key 'Q' to stop...");
            while (Console.ReadKey().Key != ConsoleKey.Q)
                continue;
            m_Stopped = true;

            for (int i = 0; i < m_MaxThreadCount; i++)
                m_Semaphore.WaitOne();
            
            Console.WriteLine("All threads quit!");
            Console.ReadKey();
        }

        static void StartTestThreads()
        {
            for (int i = 0; i < m_MaxThreadCount; i++)
            {
                m_Semaphore.WaitOne();
                var thread = new Thread(() => RunTest(i, "127.0.0.1", 911, (r, w) => RunEcho(r, w)));
                m_ThreadPool.Add(thread);
                thread.Start();
                Console.WriteLine("Thread {0} has been started!", i);
                Thread.Sleep(50);
            }
        }

        static void RunTest(int execIndex, string server, int port, Func<StreamReader, StreamWriter, bool> processSocket)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(server), port));
                var socketStream = new NetworkStream(socket);
                socketStream.ReadTimeout = 2000;//2 seconds
                socketStream.WriteTimeout = 2000;
                var socketReader = new StreamReader(socketStream, Encoding.ASCII, false);
                var socketWriter = new StreamWriter(socketStream, Encoding.ASCII, 1024);

                var welcomeLine = socketReader.ReadLine();

                while (!m_Stopped)
                {
                    if (!processSocket(socketReader, socketWriter))
                        break;

                    Thread.Sleep(100);
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
