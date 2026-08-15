using Benchmark.Data;
using Benchmark.Data.Models;
using Benchmark.Data.Repositories;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Lab.Benchmarks;

[MemoryDiagnoser]
public class List1000Scenario
{
    private const int Count = 1000;

    private AdoNetOrderRepository _adoRepo = null!;
    private DapperOrderRepository _dapperRepo = null!;
    private EfCoreOrderRepository _efRepo = null!;

    [GlobalSetup]
    public void Setup()
    {
        _adoRepo = new AdoNetOrderRepository(DbConfig.ConnectionString);
        _dapperRepo = new DapperOrderRepository(DbConfig.ConnectionString);

        var options = AppDbContextFactory.CreateOptions(DbConfig.ConnectionString);
        _efRepo = new EfCoreOrderRepository(options);
    }

    [Benchmark(Baseline = true)]
    public Task<List<Order>> AdoNetPure() => _adoRepo.GetTopAsync(Count);

    [Benchmark]
    public Task<List<Order>> Dapper() => _dapperRepo.GetTopAsync(Count);

    [Benchmark]
    public Task<List<Order>> EfCoreWithTracking() => _efRepo.GetTopWithTrackingAsync(Count);

    [Benchmark]
    public Task<List<Order>> EfCoreWithoutTracking() => _efRepo.GetTopNoTrackingAsync(Count);

    [Benchmark]
    public Task<List<Order>> EfCoreNoTrackingIdentityResolution() =>
        _efRepo.GetTopNoTrackingWithIdentityResolutionAsync(Count);
}