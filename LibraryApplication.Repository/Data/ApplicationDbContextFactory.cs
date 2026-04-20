using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LibraryApplication.Repository.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var provider = Environment.GetEnvironmentVariable("EF_PROVIDER") ?? "sqlite";

            if (provider.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
            {
                var connString = Environment.GetEnvironmentVariable("EF_CONNECTION_STRING")
                    ?? throw new InvalidOperationException("Set EF_CONNECTION_STRING env var for SQL Server migrations.");
                optionsBuilder.UseSqlServer(connString);
            }
            else
            {
                optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
