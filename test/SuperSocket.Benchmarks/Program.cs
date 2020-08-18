using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SuperSocket.Benchmarks
{
    class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
