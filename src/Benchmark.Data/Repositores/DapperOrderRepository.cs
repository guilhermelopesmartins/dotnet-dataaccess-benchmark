using Benchmark.Data.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Benchmark.Data.Repositories;

public class DapperOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public DapperOrderRepository(string connectionString) => _connectionString = connectionString;

    public async Task<Order?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT Id, CustomerName, CustomerEmail, Status, TotalAmount, CreatedAt
            FROM dbo.Orders
            WHERE Id = @Id
            """;

        await using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });
    }

    public async Task<List<Order>> GetTopAsync(int count)
    {
        const string sql = """
            SELECT TOP (@Count) Id, CustomerName, CustomerEmail, Status, TotalAmount, CreatedAt
            FROM dbo.Orders
            ORDER BY Id
            """;

        await using var connection = new SqlConnection(_connectionString);
        var result = await connection.QueryAsync<Order>(sql, new { Count = count });
        return result.AsList();
    }
}