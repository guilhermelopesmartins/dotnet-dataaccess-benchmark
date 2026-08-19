using Benchmark.Data;
using Benchmark.Data.Repositories;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Lab.Benchmarks;

/// <summary>
/// Scenario 3: full List&lt;Order&gt; vs streaming IAsyncEnumerable&lt;Order&gt;,
/// reading 50 thousand rows.
///
/// Both variants use AsNoTracking() - the change tracker cost was already isolated
/// as a separate variable in Scenario 2. Here the only difference between the two
/// benchmarks is whether the 50 thousand Order instances are all alive at the same
/// time in a List&lt;T&gt;, or pass through one by one via await foreach, being summed
/// and discarded on each iteration.
///
/// Hypothesis to confirm in the report: the materialized version should trigger
/// Gen2 collections (the internal array of List&lt;T&gt; exceeds 85 KB and lands on
/// the LOH as it doubles in size); the streaming version should not touch the LOH,
/// because there is never a collection of 50 thousand live objects at once - only
/// one Order at a time.
/// </summary>
[MemoryDiagnoser]
public class Cenario3StreamingVsMaterializacao
{
    private const int Count = 50_000;

    private EfCoreOrderRepository _efRepo = null!;

    [GlobalSetup]
    public void Setup()
    {
        var options = AppDbContextFactory.CreateOptions(DbConfig.ConnectionString);
        _efRepo = new EfCoreOrderRepository(options);
    }

    /// Materializes all 50 thousand at once (ToListAsync), then sums.
    /// The entire List&lt;Order&gt; stays alive in memory between the read and the sum.
    [Benchmark(Baseline = true)]
    public async Task<decimal> MaterializarComoLista()
    {
        var orders = await _efRepo.GetTopNoTrackingAsync(Count);

        decimal total = 0;
        foreach (var order in orders)
            total += order.TotalAmount;

        return total;
    }

    /// Streaming: each Order is summed and immediately discarded. There is never
    /// a collection with the 50 thousand objects alive at the same time - only the
    /// "order" of the current iteration, which is overwritten on the next loop turn.
    [Benchmark]
    public async Task<decimal> StreamComAwaitForeach()
    {
        decimal total = 0;

        await foreach (var order in _efRepo.StreamTopNoTrackingAsync(Count))
            total += order.TotalAmount;

        return total;
    }
}