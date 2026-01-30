using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WhiskeyTracker.Web.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }
}
