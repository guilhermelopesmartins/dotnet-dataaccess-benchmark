using Benchmark.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.Data.Repositories;

/// <summary>
/// A single DbContextOptions is reused (it's just configuration, no state),
/// but the DbContext itself is created and disposed on every call - as would
/// happen in a real HTTP request with AddDbContext (Scoped).
///
/// This is intentional: reusing the same DbContext across calls would make
/// FindAsync hit the identity map from the 2nd read of the same Id onward
/// and never touch the database again - the benchmark would look great for
/// the wrong reason.
/// </summary>
public class EfCoreOrderRepository : IOrderRepository
{
    private readonly DbContextOptions<AppDbContext> _options;

    public EfCoreOrderRepository(DbContextOptions<AppDbContext> options) => _options = options;

    public async Task<Order?> GetByIdAsync(int id)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders.FindAsync(id);
    }

    public async Task<Order?> GetByIdAsyncAsNoTracking(int id)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
    }

    /// With tracking (default): every materialized entity gets an entry
    /// in the change tracker - a snapshot of the original state plus metadata.
    public async Task<List<Order>> GetTopWithTrackingAsync(int count)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync();
    }

    /// Without tracking: materializes and returns, without keeping any snapshot.
    /// This is what should be used whenever the entity will only be read, not saved back.
    public async Task<List<Order>> GetTopNoTrackingAsync(int count)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync();
    }

    /// Middle ground: no state snapshot, but still guarantees "one row = one object"
    /// in case the same row appears twice in a JOIN (not the case here, but noted for the record).
    public async Task<List<Order>> GetTopNoTrackingWithIdentityResolutionAsync(int count)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync();
    }

    /// Streaming: because it uses 'yield return', this method does NOT execute
    /// anything until someone starts iterating (the same laziness as a regular
    /// IQueryable). The context's 'await using' is not executed ahead of time
    /// because of this - the DbContext is only created when the first
    /// MoveNextAsync() is called by the caller's await foreach, and is only
    /// disposed when the enumeration ends (or is abandoned).
    ///
    /// Each Order comes out of EF's internal SqlDataReader, is delivered via
    /// yield, and is not held in any collection in here - it's up to the
    /// consuming code to decide whether to keep it or discard it.
    public async IAsyncEnumerable<Order> StreamTopNoTrackingAsync(int count)
    {
        await using var context = new AppDbContext(_options);
 
        var query = context.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Take(count)
            .AsAsyncEnumerable();
 
        await foreach (var order in query)
            yield return order;
    }
}