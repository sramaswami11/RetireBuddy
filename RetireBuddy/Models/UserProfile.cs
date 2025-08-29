using System.ComponentModel.DataAnnotations;

namespace RetireBuddy.Models
{
    public class UserProfile
    {
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public FilingStatus FilingStatus { get; set; } = FilingStatus.MarriedFilingJointly;

        // Current-year earned/other taxable income (excludes SS; we'll compute SS tax separately)
        [Range(0, double.MaxValue)]
        public decimal TaxableIncome { get; set; } = 0m;

        // Social Security gross annual benefit (if receiving)
        [Range(0, double.MaxValue)]
        public decimal SocialSecurityIncome { get; set; } = 0m;

        // Tax-exempt interest (e.g., muni bonds) — counts for IRMAA and SS provisional income
        [Range(0, double.MaxValue)]
        public decimal TaxExemptInterest { get; set; } = 0m;

        // Retirement age (planning input)
        [Range(40, 80)]
        public int RetirementAge { get; set; } = 65;

        public decimal StateTaxRate { get; set; }

        public bool IsRetiredInYear(int year)
        {
            int age = GetAgeOnDec31(year);
            return age >= RetirementAge;
        }

        public int GetAgeOnDec31(int year)
        {
            var dec31 = new DateTime(year, 12, 31);
            int age = dec31.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > dec31.AddYears(-age)) age--;
            return age;
        }
    }

    public enum FilingStatus
    {
        Single,
        MarriedFilingJointly,
        MarriedFilingSeparately,
        HeadOfHousehold
    }
}
