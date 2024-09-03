using Microsoft.EntityFrameworkCore;
using Shopizy.Ordering.API.Persistence;

namespace Shopizy.Ordering.API;

public class DbMigrationsHelper(
    ILogger<DbMigrationsHelper> logger,
    OrderDbContext context
    )
{
    public async Task MigrateAsync()
    {
        try
        {
            if (context.Database.IsSqlServer() && (await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}
