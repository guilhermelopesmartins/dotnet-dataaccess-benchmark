using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Bogus;
using Microsoft.Data.SqlClient;

namespace Benchmark.Lab.Seed;

/// <summary>
/// Generates Orders + OrderItems with Bogus and inserts them via SqlBulkCopy.
/// Order Ids are assigned in memory (not left to IDENTITY) so that
/// OrderItems.OrderId is already correct from the start, without a round-trip to the database.
/// </summary>
public static class DatabaseSeeder
{
    private static readonly string[] Statuses = { "Pending", "Shipped", "Delivered", "Cancelled" };

    public static void Run(string connectionString, int orderCount = 200_000)
    {
        var sw = Stopwatch.StartNew();
        Randomizer.Seed = new Random(42);

        var customerFaker = new Faker("pt_BR");
        var productFaker = new Faker();

        var orders = new DataTable();
        orders.Columns.Add("Id", typeof(int));
        orders.Columns.Add("CustomerName", typeof(string));
        orders.Columns.Add("CustomerEmail", typeof(string));
        orders.Columns.Add("Status", typeof(string));
        orders.Columns.Add("TotalAmount", typeof(decimal));
        orders.Columns.Add("CreatedAt", typeof(DateTime));

        var items = new DataTable();
        items.Columns.Add("OrderId", typeof(int));
        items.Columns.Add("ProductName", typeof(string));
        items.Columns.Add("Quantity", typeof(int));
        items.Columns.Add("UnitPrice", typeof(decimal));

        var rnd = new Random(42);

        Console.WriteLine($"Generating {orderCount:N0} orders and their items in memory...");

        for (int orderId = 1; orderId <= orderCount; orderId++)
        {
            int itemCount = rnd.Next(3, 9); // 3 - 8
            decimal orderTotal = 0m;

            for (int i = 0; i < itemCount; i++)
            {
                int quantity = rnd.Next(1, 6);
                decimal unitPrice = Math.Round((decimal)(rnd.NextDouble() * 490 + 10), 2); // $ 10 - 500
                orderTotal += quantity * unitPrice;

                items.Rows.Add(orderId, productFaker.Commerce.ProductName(), quantity, unitPrice);
            }

            orders.Rows.Add(
                orderId,
                customerFaker.Name.FullName(),
                customerFaker.Internet.Email(),
                Statuses[rnd.Next(Statuses.Length)],
                orderTotal,
                DateTime.UtcNow.AddDays(-rnd.Next(0, 365)));

            if (orderId % 20_000 == 0)
                Console.WriteLine($"  {orderId:N0}/{orderCount:N0} orders generated in memory...");
        }

        Console.WriteLine($"In-memory generation completed in {sw.Elapsed}. " +
                           $"Orders: {orders.Rows.Count:N0} | OrderItems: {items.Rows.Count:N0}");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        BulkInsert(connection, orders, "dbo.Orders", SqlBulkCopyOptions.KeepIdentity);
        Console.WriteLine($"Orders inserted. Total time: {sw.Elapsed}");

        BulkInsert(connection, items, "dbo.OrderItems", SqlBulkCopyOptions.Default);
        Console.WriteLine($"OrderItems inserted. Total time: {sw.Elapsed}");

        Console.WriteLine("Seed completed.");
    }

    private static void BulkInsert(SqlConnection connection, DataTable table, string destinationTable, SqlBulkCopyOptions options)
    {
        using var bulkCopy = new SqlBulkCopy(connection, options, externalTransaction: null)
        {
            DestinationTableName = destinationTable,
            BatchSize = 10_000,
            BulkCopyTimeout = 120,
            EnableStreaming = true
        };

        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        bulkCopy.WriteToServer(table);
    }
}