using Microsoft.EntityFrameworkCore;
using Shopizy.Ordering.API.Persistence;

namespace Shopizy.Ordering.API;

public class DbMigrationsHelper(
    ILogger<DbMigrationsHelper> logger,
    OrderDbContext context
    )
{
    private readonly OrderDbContext _context = context;
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
