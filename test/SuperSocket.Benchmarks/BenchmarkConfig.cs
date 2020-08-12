using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace SuperSocket.Benchmarks
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            Add(DefaultConfig.Instance);
            AddDiagnoser(MemoryDiagnoser.Default);
            this.WithOption(ConfigOptions.DisableOptimizationsValidator, true);
        }
    }

}