using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Services;

public class LegacyMigrationService
{
    private readonly AppDbContext _context;

    public LegacyMigrationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnsureUserHasCollectionAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        // Optimization: We now create the default collection at Registration time.
        // We no longer check for it on every request.
        
        // Use this only if we need a manual "Fix My Account" button or similar.
        // For the hot path, this is now a no-op or removed.
    }
}
