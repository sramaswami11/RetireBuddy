using RetireBuddy.Models;

namespace RetireBuddy.Services;

public class IrmaaService
{
    // Only the first-tier ceiling is needed to compute a "safe max conversion" target.
    // These values change annually and use a 2-year lookback. Expose as inputs.
    public decimal GetFirstTierCeiling(FilingStatus status, PlannerInput input)
    {
        return status == FilingStatus.MarriedFilingJointly ? input.IrmaaFirstTierCeilingMFJ : input.IrmaaFirstTierCeilingSingle;
    }

    public string GetTierNameByMAGI(FilingStatus status, decimal magi, PlannerInput input)
    {
        var ceiling = GetFirstTierCeiling(status, input);
        if (magi <= ceiling) return "Standard";
        // For simplicity we only label if above standard; detailed tiers can be added.
        return "> Standard";
    }
}
