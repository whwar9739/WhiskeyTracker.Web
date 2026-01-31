using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiskeyTracker.Web.Data;

namespace WhiskeyTracker.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CollectionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(int id)
    {
        var userId = _userManager.GetUserId(User);

        // Security: Check if current user is a member of this collection
        var isMember = await _context.CollectionMembers
            .AnyAsync(m => m.CollectionId == id && m.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        // Fetch all members of the collection
        var memberIds = await _context.CollectionMembers
            .Where(m => m.CollectionId == id)
            .Select(m => m.UserId)
            .ToListAsync();

        var users = await _userManager.Users
            .Where(u => memberIds.Contains(u.Id))
            .ToListAsync();

        var result = users.Select(u => new
        {
            id = u.Id,
            name = string.IsNullOrEmpty(u.DisplayName) ? u.UserName : u.DisplayName
        });

        return Ok(result);
    }
}
