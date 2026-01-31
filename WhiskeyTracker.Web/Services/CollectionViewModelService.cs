using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace WhiskeyTracker.Web.Services;

public class CollectionViewModelService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CollectionViewModelService(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<(SelectList Collections, SelectList Purchasers)> GetDropdownsAsync(string userId, int? selectedCollectionId = null, string? selectedPurchaserId = null)
    {
        // 1. Get My Collections
        var myMemberships = await _context.CollectionMembers
            .Include(m => m.Collection)
            .Where(m => m.UserId == userId)
            .ToListAsync();
        
        var collectionsList = new SelectList(myMemberships.Select(m => m.Collection), "Id", "Name", selectedCollectionId);

        // 2. Get Potential Purchasers (All members of my collections)
        var myCollectionIds = myMemberships.Select(m => m.CollectionId).ToList();

        var allMemberUserIds = await _context.CollectionMembers
            .Where(m => myCollectionIds.Contains(m.CollectionId))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();

        var users = await _userManager.Users
            .Where(u => allMemberUserIds.Contains(u.Id))
            .ToListAsync();
        
        // Format names nicely
        var purchaserList = users.Select(u => new 
        { 
            Id = u.Id, 
            Name = string.IsNullOrEmpty(u.DisplayName) ? u.UserName : u.DisplayName 
        }).ToList();

        // Default purchaser logic: if selectedPurchaserId is null, default to current userId (if they are in the list)
        var defaultPurchaser = selectedPurchaserId ?? userId;

        var purchasersList = new SelectList(purchaserList, "Id", "Name", defaultPurchaser);

        return (collectionsList, purchasersList);
    }
}
