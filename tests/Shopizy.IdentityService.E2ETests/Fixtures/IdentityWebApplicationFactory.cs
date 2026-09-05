using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shopizy.IdentityService.Infrastructure.Persistence;

namespace Shopizy.IdentityService.E2ETests.Fixtures;

public class IdentityWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"E2EDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Register fresh in-memory database
            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}
