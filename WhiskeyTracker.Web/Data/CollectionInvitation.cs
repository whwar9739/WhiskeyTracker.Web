using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WhiskeyTracker.Web.Data;

public enum InvitationStatus
{
    Pending,
    Accepted,
    Declined
}

public class CollectionInvitation
{
    public int Id { get; set; }

    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;

    [Required]
    public string InviterUserId { get; set; } = string.Empty;
    public ApplicationUser InviterUser { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string InviteeEmail { get; set; } = string.Empty;

    public CollectionRole Role { get; set; } = CollectionRole.Viewer;

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
