using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UknfPlatform.Infrastructure.Persistence.Contexts;

namespace UknfPlatform.Tests.Integration.Api.Auth;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public IntegrationTestWebApplicationFactory()
    {
        // Use a unique in-memory database for each test run
        _connectionString = $"Host=localhost;Port=5432;Database=uknf_platform_test_{Guid.NewGuid()};Username=postgres;Password=postgres";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase($"InMemoryTestDb_{Guid.NewGuid()}");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            }
        });

        builder.UseEnvironment("Testing");
    }
}

