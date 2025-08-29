using System.ComponentModel.DataAnnotations;

namespace RetireBuddy.Models;

public class PlannerInput
{
    public UserProfile Profile { get; set; } = new();
    public ConversionPlan Plan { get; set; } = new();

    // Simple state handling: flat state tax rate. (Extend with per-state tables later.)
    [Range(0, 0.2)]
    public decimal StateTaxRate { get; set; } = 0m; // e.g., 0.05m for 5%

    // For IRMAA planning, allow overriding the first-tier ceiling (varies by year)
    public decimal IrmaaFirstTierCeilingMFJ { get; set; } = 206000m; // configurable
    public decimal IrmaaFirstTierCeilingSingle { get; set; } = 103000m; // configurable
}
