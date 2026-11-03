
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AdminBareSkin.Data
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environmentConnection =
                Environment.GetEnvironmentVariable(
                    "ConnectionStrings__DefaultConnection");

            string? connectionString = environmentConnection;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddUserSecrets(
                        typeof(ApplicationDbContext).Assembly,
                        optional: true)
                    .Build();

                connectionString =
                    configuration.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No se encontró la conexión a Supabase.");
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}
