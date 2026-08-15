using Benchmark.Data;
using Benchmark.Data.Models;
using Benchmark.Data.Repositories;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Lab.Benchmarks;

[MemoryDiagnoser]
public class GetByIdScenario
{
    // Id no meio do intervalo gerado pelo seed (1..200_000) - qualquer um serve,
    // o que importa é ser o MESMO em todas as três implementações.
    private const int OrderId = 100_000;

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
    public Task<Order?> AdoNetPure() => _adoRepo.GetByIdAsync(OrderId);

    [Benchmark]
    public Task<Order?> Dapper() => _dapperRepo.GetByIdAsync(OrderId);

    [Benchmark]
    public Task<Order?> EfCore() => _efRepo.GetByIdAsync(OrderId);
    [Benchmark]
    public Task<Order?> EfCoreNoTracking() => _efRepo.GetByIdAsyncAsNoTracking(OrderId);
}