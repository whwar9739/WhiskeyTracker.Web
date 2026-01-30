using System.ComponentModel.DataAnnotations;

namespace WhiskeyTracker.Web.Data;

public class Collection
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public List<CollectionMember> Members { get; set; } = new();

    public List<Bottle> Bottles { get; set; } = new();
}
