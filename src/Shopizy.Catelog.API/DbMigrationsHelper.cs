using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Persistence;

namespace Shopizy.Catelog.API;

public class DbMigrationsHelper(
    ILogger<DbMigrationsHelper> logger,
    CatelogDbContext context
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
