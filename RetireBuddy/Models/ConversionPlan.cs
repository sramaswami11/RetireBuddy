namespace RetireBuddy.Models
{
    public enum ConversionStrategy
    {
        OneShot,
        EvenSpread
    }

    public class ConversionPlan
    {
        public UserProfile Profile { get; set; }   // 🔑 attach profile here
        public decimal TotalAmountToConvert { get; set; } = 0m;
        public ConversionStrategy Strategy { get; set; } = ConversionStrategy.EvenSpread;

        // Number of years over which to spread the conversion
        // If 1 → one-shot, if >1 → even split across years
        public int NumberOfYears { get; set; } = 1;

        // Optional target age to finish (e.g., 73 for pre-RMD conversions)
        public int TargetCompletionAge { get; set; } = 73;
    }
}
