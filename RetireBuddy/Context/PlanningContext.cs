using RetireBuddy.Models;

namespace RetireBuddy.Context
{
    public class PlanningContext
    {
        //public UserProfile UserProfile { get; set; }
        //public ConversionPlan ConversionPlan { get; set; }
        public PlannerInput Input { get; set; } = new();
        public List<TaxYearModel> TaxProjections { get; set; } = new();
    }
}
