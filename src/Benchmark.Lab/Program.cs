using Benchmark.Lab.Seed;

const string ConnectionString =
    "Server=localhost,1433;Database=BenchmarkDb;User Id=sa;Password=Benchmark@2026;TrustServerCertificate=True";

if (args.Length > 0 && args[0] == "seed")
{
    DatabaseSeeder.Run(ConnectionString);
    return;
}

Console.WriteLine("Nenhum benchmark configurado ainda. Rode com o argumento 'seed' para popular o banco.");