using Benchmark.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Benchmark.Data;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Mapeamento explícito: o schema já existe (criado via SQL puro em 001_schema.sql),
    // não via Migrations do EF. Sem isso, as convenções do EF poderiam divergir do banco real.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerName).HasMaxLength(100).IsRequired();
            entity.Property(o => o.CustomerEmail).HasMaxLength(150).IsRequired();
            entity.Property(o => o.Status).HasMaxLength(20).IsRequired();
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
        });
    }
}

public static class AppDbContextFactory
{
    public static DbContextOptions<AppDbContext> CreateOptions(string connectionString) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;
}