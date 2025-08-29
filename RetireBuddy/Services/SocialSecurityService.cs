using RetireBuddy.Models;

namespace RetireBuddy.Services;

public class SocialSecurityService
{
    // Computes taxable SS using provisional income method
    // Thresholds (unchanged since 1980s): Single 25k/34k; MFJ 32k/44k
    public decimal ComputeTaxableSS(FilingStatus status, decimal grossSS, decimal otherAGIExclSS, decimal taxExemptInterest)
    {
        if (grossSS <= 0) return 0m;
        var halfSS = 0.5m * grossSS;
        decimal base1, base2;
        switch (status)
        {
            case FilingStatus.Single:
            case FilingStatus.HeadOfHousehold:
            case FilingStatus.MarriedFilingSeparately:
                base1 = 25000m; base2 = 34000m; break;
            case FilingStatus.MarriedFilingJointly:
            default:
                base1 = 32000m; base2 = 44000m; break;
        }
        var provisional = otherAGIExclSS + taxExemptInterest + halfSS;
        if (provisional <= base1) return 0m;
        if (provisional <= base2)
        {
            var taxable = 0.5m * (provisional - base1);
            return Math.Min(taxable, 0.5m * grossSS);
        }
        else
        {
            var amountOverBase2 = provisional - base2;
            var taxable = 0.85m * amountOverBase2 + Math.Min(0.5m * (base2 - base1), 0.5m * grossSS);
            return Math.Min(taxable, 0.85m * grossSS);
        }
    }
}
