using Microsoft.EntityFrameworkCore;
using Shopizy.Catelog.API.Persistence;

namespace Shopizy.Catelog.API;

public class DbMigrationsHelper(
    ILogger<DbMigrationsHelper> logger,
    CatelogDbContext context
    )
{
    private readonly CatelogDbContext _context = context;
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
