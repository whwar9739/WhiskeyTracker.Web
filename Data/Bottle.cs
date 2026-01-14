using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhiskeyTracker.Web.Data;

public enum BottleStatus
{
    Full,
    Opened,
    Empty
}

public class Bottle
{
    public int Id { get; set; }

    // --- The Link to the Whiskey Definition ---
    public int WhiskeyId { get; set; } // Foreign Key
    public Whiskey Whiskey { get; set; } = null!; // Navigation Property

    // --- Inventory Details ---
    [DataType(DataType.Date)]
    public DateOnly? PurchaseDate { get; set; }

    [DataType(DataType.Currency)]
    public decimal? PurchasePrice { get; set; }

    [MaxLength(100)]
    public string PurchaseLocation { get; set; } = string.Empty;

    public BottleStatus Status { get; set; } = BottleStatus.Full;
    
    // Optional: You might want to track specific bottling dates for specific bottles
    public DateOnly? BottlingDate { get; set; } 
}