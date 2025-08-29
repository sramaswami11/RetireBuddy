using RetireBuddy.Models;

namespace RetireBuddy.Services
{
    public class TaxCalculationService
    {
        // 2025-ish brackets (example) — provide MFJ and Single; can extend
        private readonly List<(decimal Limit, decimal Rate)> _mfjBrackets = new()
    {
        (23850m, 0.10m),
        (96950m, 0.12m),
        (206700m, 0.22m),
        (394600m, 0.24m),
        (501050m, 0.32m),
        (751600m, 0.35m),
        (decimal.MaxValue, 0.37m)
    };

        private readonly List<(decimal Limit, decimal Rate)> _singleBrackets = new()
    {
        (11925m, 0.10m),
        (48475m, 0.12m),
        (103350m, 0.22m),
        (197300m, 0.24m),
        (250525m, 0.32m),
        (375800m, 0.35m),
        (decimal.MaxValue, 0.37m)
    };

        public List<(decimal Limit, decimal Rate)> GetBrackets(FilingStatus status) =>
            status == FilingStatus.MarriedFilingJointly ? _mfjBrackets : _singleBrackets;

        public decimal CalculateFederalTax(FilingStatus status, decimal taxableIncome)
        {
            var brackets = GetBrackets(status);
            decimal remaining = taxableIncome;
            decimal tax = 0m;
            decimal lower = 0m;
            foreach (var (limit, rate) in brackets)
            {
                var slice = Math.Min(remaining, limit - lower);
                if (slice <= 0) break;
                tax += slice * rate;
                remaining -= slice;
                lower = limit;
            }
            return tax;
        }

        public decimal GetNextBracketCeiling(FilingStatus status, decimal currentTaxableIncome)
        {
            var brackets = GetBrackets(status);
            foreach (var (limit, rate) in brackets)
            {
                if (currentTaxableIncome < limit)
                    return limit; // first limit above current income
            }
            return decimal.MaxValue;
        }

        public decimal CalculateMAGI(decimal taxableIncome, decimal socialSecurityIncome, decimal rothConversion)
        {
            return taxableIncome + (socialSecurityIncome * 0.85m) + rothConversion;
        }

        // 🆕 Helper: calculate total tax (federal + state)
        public decimal CalculateTotalTax(FilingStatus filingStatus, decimal taxableIncome, decimal socialSecurityIncome, decimal rothConversion, decimal stateTaxRate)
        {
            decimal magi = CalculateMAGI(taxableIncome, socialSecurityIncome, rothConversion);

            decimal federalTax = CalculateFederalTax(filingStatus, magi);
            decimal stateTax = magi * stateTaxRate;

            return federalTax + stateTax;
        }
    }
}
