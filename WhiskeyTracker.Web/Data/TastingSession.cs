using System.ComponentModel.DataAnnotations;

namespace WhiskeyTracker.Web.Data;

public class TastingSession
{
    public int Id { get; set; }

    [Required]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    public List<TastingNote> Notes { get; set; } = new();
}