using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WhiskeyTracker.Web.Data;

public enum CollectionRole
{
    Owner,
    Editor,
    Viewer
}

public class CollectionMember
{
    public int Id { get; set; }

    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public CollectionRole Role { get; set; } = CollectionRole.Viewer;
}
