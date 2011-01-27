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

            GC.Collect();

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
