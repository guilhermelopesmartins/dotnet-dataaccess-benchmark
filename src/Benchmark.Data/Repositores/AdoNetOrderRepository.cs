using Benchmark.Data.Models;
using Microsoft.Data.SqlClient;

namespace Benchmark.Data.Repositories;

/// <summary>
/// O piso do benchmark: SqlCommand + SqlDataReader diretos, sem abstração.
/// Nova conexão por chamada, apoiada no connection pooling do ADO.NET -
/// é o que qualquer camada acima dele faz por baixo dos panos de qualquer forma.
/// </summary>
public class AdoNetOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public AdoNetOrderRepository(string connectionString) => _connectionString = connectionString;

    public async Task<Order?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT Id, CustomerName, CustomerEmail, Status, TotalAmount, CreatedAt
            FROM dbo.Orders
            WHERE Id = @Id
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return Map(reader);
    }

    public async Task<List<Order>> GetTopAsync(int count)
    {
        const string sql = """
            SELECT TOP (@Count) Id, CustomerName, CustomerEmail, Status, TotalAmount, CreatedAt
            FROM dbo.Orders
            ORDER BY Id
            """;

        var result = new List<Order>(count);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Count", count);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(Map(reader));

        return result;
    }

    private static Order Map(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        CustomerName = reader.GetString(1),
        CustomerEmail = reader.GetString(2),
        Status = reader.GetString(3),
        TotalAmount = reader.GetDecimal(4),
        CreatedAt = reader.GetDateTime(5)
    };
}