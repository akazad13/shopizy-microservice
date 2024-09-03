using Microsoft.EntityFrameworkCore;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API;

public class DbMigrationsHelper(
    ILogger<DbMigrationsHelper> logger,
    CartDbContext context
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
