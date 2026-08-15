using Benchmark.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.Data.Repositories;

/// <summary>
/// Um DbContextOptions é reaproveitado (é só configuração, sem estado),
/// mas o DbContext em si nasce e morre a cada chamada - como aconteceria
/// numa requisição HTTP real com AddDbContext (Scoped).
///
/// Isso é proposital: reaproveitar o mesmo DbContext entre chamadas faria
/// o FindAsync acertar o identity map na 2ª leitura do mesmo Id em diante
/// e nunca mais ir ao banco - o benchmark pareceria ótimo por um motivo errado.
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

    /// Com tracking (padrão): cada entidade materializada ganha uma entrada
    /// no change tracker - snapshot do estado original + metadados.
    public async Task<List<Order>> GetTopWithTrackingAsync(int count)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync();
    }

    /// Sem tracking: materializa e entrega, sem guardar snapshot nenhum.
    /// É o que se deve usar sempre que a entidade só vai ser lida, não salva de volta.
    public async Task<List<Order>> GetTopNoTrackingAsync(int count)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders
            .AsNoTracking()
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync();
    }

    /// Meio-termo: sem snapshot de estado, mas ainda garante "uma linha = um objeto"
    /// caso a mesma linha apareça duas vezes num JOIN (não é o caso aqui, mas fica o registro).
    public async Task<List<Order>> GetTopNoTrackingWithIdentityResolutionAsync(int count)
    {
        await using var context = new AppDbContext(_options);
        return await context.Orders
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(o => o.Id)
            .Take(count)
            .ToListAsync();
    }
}