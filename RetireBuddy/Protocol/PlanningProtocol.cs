// Protocol/PlanningProtocol.cs
using RetireBuddy.Context;
using RetireBuddy.Models;
using RetireBuddy.Services;

public class PlanningProtocol
{
    private readonly PlanningContext _context;
    private readonly TaxCalculationService _tax;
    private readonly SocialSecurityService _ss;
    private readonly IrmaaService _irmaa;

    public PlanningProtocol(PlanningContext ctx, TaxCalculationService tax, SocialSecurityService ss, IrmaaService irmaa)
    {
        _context = ctx; _tax = tax; _ss = ss; _irmaa = irmaa;
    }

    public void SetPlan(PlannerInput input)
    {
        _context.Input = input;
        _context.TaxProjections.Clear();
    }

    public IReadOnlyList<TaxYearModel> TaxProjections => _context.TaxProjections;

    // Computes max Roth conversion for the CURRENT year that does NOT
    // (a) push MAGI above IRMAA first-tier ceiling, nor (b) cross to next tax bracket.
    //public decimal ComputeMaxSafeConversionForCurrentYear(int year)
    //{
    //    var p = _context.Input.Profile;
    //    var status = p.FilingStatus;
    //    bool retired = p.IsRetiredInYear(year);

    //    // Base non-SS taxable income (wages, interest, etc.) for the year
    //    var baseTaxable = p.TaxableIncome;

    //    // Compute taxable SS given other income but excluding the conversion for now
    //    var taxableSS = _ss.ComputeTaxableSS(status, p.SocialSecurityIncome, otherAGIExclSS: baseTaxable, taxExemptInterest: p.TaxExemptInterest);

    //    // Current taxable income before conversion
    //    var currentTaxableIncome = baseTaxable + taxableSS; // (Ignoring std deduction for simplicity)

    //    // Room until the next federal bracket ceiling
    //    var nextBracketCeiling = _tax.GetNextBracketCeiling(status, currentTaxableIncome);
    //    var roomToBracket = Math.Max(0m, nextBracketCeiling - currentTaxableIncome);

    //    // IRMAA MAGI = AGI + tax-exempt interest (AGI includes taxable SS). Conversion increases AGI dollar-for-dollar
    //    var currentMAGI = currentTaxableIncome + p.TaxExemptInterest;
    //    var irmaaCeiling = _irmaa.GetFirstTierCeiling(status, _context.Input);
    //    var roomToIrmaa = Math.Max(0m, irmaaCeiling - currentMAGI);

    //    // The conversion increases both taxable income and MAGI equally (ignoring deductions), so safe max is min of both rooms
    //    var safeMax = Math.Min(roomToBracket, roomToIrmaa);
    //    return safeMax < 0 ? 0 : safeMax;
    //}

    public decimal CalculateMaxSafeConversion(UserProfile profile, ConversionPlan plan)
    {
        var currentYear = new TaxYearModel
        {
            Year = DateTime.Now.Year,
            TaxableIncome = profile.TaxableIncome,
            AdjustedGrossIncome = profile.TaxableIncome + profile.SocialSecurityIncome
        };

        return CalculateMaxSafeConversion(profile, currentYear, plan);
    }


    public decimal CalculateMaxSafeConversion(UserProfile profile, TaxYearModel year, ConversionPlan plan)
    {
        // --- Step 1: Start from current AGI ---
        decimal currentAgi = year.TaxableIncome;

        // --- Step 2: Find next federal tax bracket threshold ---
        var brackets = TaxTables.GetBrackets(profile.FilingStatus, year.Year);
        decimal nextBracketLimit = brackets
            .Where(b => currentAgi < b.Threshold)
            .Select(b => b.Threshold)
            .DefaultIfEmpty(currentAgi) // if already at top bracket
            .First();

        decimal roomBeforeNextBracket = nextBracketLimit - currentAgi;

        // --- Step 3: Check IRMAA threshold ---
        decimal irmaaThreshold = IrmaaTables.GetThreshold(profile.FilingStatus, year.Year);
        decimal roomBeforeIrmaa = irmaaThreshold - currentAgi;

        // --- Step 4: Max safe conversion is the smaller of the two ---
        decimal safeConversion = Math.Min(roomBeforeNextBracket, roomBeforeIrmaa);

        // --- Step 5: Don’t exceed plan’s total ---
        safeConversion = Math.Min(safeConversion, plan.TotalAmountToConvert);

        return safeConversion < 0 ? 0 : safeConversion;
    }


    //public void SimulatePlan()
    //{
    //    var input = _context.Input;
    //    var profile = input.Profile;
    //    var plan = input.Plan;
    //    int startYear = DateTime.Now.Year;

    //    // Determine timeline based on target completion age
    //    int startAge = profile.GetAgeOnDec31(startYear);
    //    int yearsToTarget = Math.Max(1, plan.TargetCompletionAge - startAge);
    //    int totalYears = plan.Strategy == ConversionStrategy.OneShot ? 1 : yearsToTarget;
    //    decimal perYear = plan.Strategy == ConversionStrategy.OneShot
    //        ? plan.TotalAmountToConvert
    //        : (yearsToTarget > 0 ? plan.TotalAmountToConvert / yearsToTarget : 0m);

    //    _context.TaxProjections.Clear(); // clear previous results

    //    for (int i = 0; i < totalYears; i++)
    //    {
    //        int year = startYear + i;
    //        int age = profile.GetAgeOnDec31(year);

    //        // Use input values to ensure taxable income is correct
    //        var otherTaxable = profile.TaxableIncome; // user input
    //        var socialSecurity = profile.SocialSecurityIncome; // user input

    //        // Compute taxable SS before conversion
    //        var taxableSS_pre = _ss.ComputeTaxableSS(profile.FilingStatus, socialSecurity, otherTaxable, profile.TaxExemptInterest);

    //        // Add conversion for this year
    //        var conversion = perYear;

    //        // Recompute taxable SS with conversion included (affects provisional income)
    //        var taxableSS = _ss.ComputeTaxableSS(profile.FilingStatus, socialSecurity, otherTaxable + conversion, profile.TaxExemptInterest);

    //        // Compute AGI, taxable income, federal & state taxes
    //        var agi = otherTaxable + taxableSS + conversion; // simplified AGI
    //        var taxableIncome = agi; // same simplification
    //        var federalTax = _tax.CalculateFederalTax(profile.FilingStatus, taxableIncome);
    //        var stateTax = taxableIncome * input.StateTaxRate;
    //        var magi = agi + profile.TaxExemptInterest;
    //        var tier = _irmaa.GetTierNameByMAGI(profile.FilingStatus, magi, input);

    //        // Add year to projections
    //        _context.TaxProjections.Add(new TaxYearModel
    //        {
    //            Year = year,
    //            Age = age,
    //            RothConversion = conversion,
    //            TaxableIncome = magi,
    //            OtherTaxableIncome = otherTaxable,
    //            TaxableSocialSecurity = taxableSS,
    //            TaxExemptInterest = profile.TaxExemptInterest,
    //            AdjustedGrossIncome = agi,
    //            FederalTaxableIncome = taxableIncome,
    //            FederalTax = federalTax,
    //            StateTax = stateTax,
    //            MAGI_ForIrmaa = magi,
    //            IrmaaTier = tier
    //        });
    //    }
    //}

    public void SimulatePlan()
    {
        var input = _context.Input;
        var profile = input.Profile;
        var plan = input.Plan;
        int startYear = DateTime.Now.Year;

        // --- timeline logic per your rule ---
        int ageThisYear = profile.GetAgeOnDec31(startYear);
        int yearsUntil73 = Math.Max(0, 73 - ageThisYear);

        int requestedYears = plan.Strategy == ConversionStrategy.OneShot
            ? 1
            : Math.Max(1, plan.NumberOfYears); // never < 1

        int totalYears;
        if (plan.Strategy == ConversionStrategy.OneShot)
        {
            totalYears = 1;
        }
        else
        {
            // If under 73, cap to yearsUntil73 when user requests more; otherwise use their request
            totalYears = (yearsUntil73 > 0)
                ? Math.Min(requestedYears, yearsUntil73)
                : requestedYears; // already at/past 73 -> honor requestedYears
        }

        totalYears = Math.Max(1, totalYears); // safety

        // Per-year conversion
        decimal perYear = plan.Strategy == ConversionStrategy.OneShot
            ? plan.TotalAmountToConvert
            : (totalYears > 0 ? plan.TotalAmountToConvert / totalYears : 0m);

        // --- simulate each year ---
        for (int i = 0; i < totalYears; i++)
        {
            int year = startYear + i;
            int age = profile.GetAgeOnDec31(year);

            // Base incomes
            var otherTaxable = profile.TaxableIncome;

            // Taxable SS depends on other income + conversion
            var taxableSS_pre = _ss.ComputeTaxableSS(
                profile.FilingStatus,
                profile.SocialSecurityIncome,
                otherTaxable,
                profile.TaxExemptInterest);

            var conversion = perYear;

            // Recompute taxable SS including the conversion
            var taxableSS = _ss.ComputeTaxableSS(
                profile.FilingStatus,
                profile.SocialSecurityIncome,
                otherTaxable + conversion,
                profile.TaxExemptInterest);

            var agi = otherTaxable + taxableSS + conversion;   // simplified (no deductions)
            var taxableIncome = agi;                           // same simplification
            var federalTax = _tax.CalculateFederalTax(profile.FilingStatus, taxableIncome);
            var stateTax = taxableIncome * input.StateTaxRate;
            var magi = agi + profile.TaxExemptInterest;
            var tier = _irmaa.GetTierNameByMAGI(profile.FilingStatus, magi, input);

            _context.TaxProjections.Add(new TaxYearModel
            {
                Year = year,
                Age = age,
                RothConversion = conversion,
                OtherTaxableIncome = otherTaxable,
                TaxableSocialSecurity = taxableSS,
                TaxExemptInterest = profile.TaxExemptInterest,
                AdjustedGrossIncome = agi,
                FederalTaxableIncome = taxableIncome,
                FederalTax = federalTax,
                StateTax = stateTax,
                MAGI_ForIrmaa = magi,
                IrmaaTier = tier
            });
        }
    }


}

public static class TaxTables
{
    public static List<(decimal Threshold, decimal Rate)> GetBrackets(FilingStatus status, int year)
    {
        // Example 2025 married-filing-jointly brackets (partial, stub values)
        return new List<(decimal, decimal)>
        {
            (22000m, 0.10m),
            (89450m, 0.12m),
            (190750m, 0.22m),
            (364200m, 0.24m),
            (462500m, 0.32m),
            (693750m, 0.35m),
            (decimal.MaxValue, 0.37m)
        };
    }
}

public static class IrmaaTables
{
    public static decimal GetThreshold(FilingStatus status, int year)
    {
        // Example: 2025 IRMAA married joint first tier = $206,000
        if (status == FilingStatus.MarriedFilingJointly)
            return 206000m;
        else
            return 103000m; // single
    }
}

