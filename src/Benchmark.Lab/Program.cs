using Benchmark.Data;
using Benchmark.Lab.Benchmarks;
using Benchmark.Lab.Seed;
using BenchmarkDotNet.Running;

if (args.Length > 0 && args[0] == "seed")
{
    DatabaseSeeder.Run(DbConfig.ConnectionString);
    return;
}

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);