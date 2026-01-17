using System.ComponentModel.DataAnnotations;

namespace WhiskeyTracker.Web.Data;

public class BlendComponent
{
    public int Id { get; set; }

    // The Infinity Bottle receiving the liquid
    public int InfinityBottleId { get; set; }
    public Bottle? InfinityBottle { get; set; }

    // The source bottle providing the liquid
    public int SourceBottleId { get; set; }
    public Bottle? SourceBottle { get; set; }

    [Required]
    [Range(1, 1000)]
    public int AmountAddedMl { get; set; }

    public DateOnly DateAdded { get; set; }
}