using Microsoft.EntityFrameworkCore;
using Shopizy.Cart.API.Persistence;

namespace Shopizy.Cart.API;

public class DbMigrationsHelper(
    ILogger<DbMigrationsHelper> logger,
    CartDbContext context
    )
{
    private readonly CartDbContext _context = context;
    private readonly ILogger<DbMigrationsHelper> _logger = logger;
    public async Task MigrateAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer() && (await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.DatabaseInitializationError(ex);
            throw;
        }
    }
}
