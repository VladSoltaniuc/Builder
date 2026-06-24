// Infrastructure — spins up the real app against a test database
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;

namespace ProductApi.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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
