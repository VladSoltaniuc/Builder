// Infrastructure - spins up the real app against a test database
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;

namespace ProductApi.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    // Stable test credentials used by IntegrationTestBase to obtain an admin token
    public const string AdminEmail    = "ci-admin@test.local";
    public const string AdminPassword = "CiAdmin12345!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Inject test-only config before services are built
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminSeed:Email"]       = AdminEmail,
                ["AdminSeed:Password"]    = AdminPassword,
                ["AdminSeed:Name"]        = "CI Admin",
                // Disabled so rapid back-to-back test requests don't hit the 429 limit
                ["RateLimiting:Enabled"]  = "false",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the real DB with the test DB
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            var connectionString = Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
                ?? throw new InvalidOperationException("TEST_CONNECTION_STRING environment variable is not set.");

            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

            // Ensure test DB is created and migrated
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        });
    }
}
