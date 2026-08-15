using Benchmark.Data.Models;

namespace Benchmark.Data.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
}