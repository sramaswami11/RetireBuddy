namespace RetireBuddy.Models
{
    public class TaxYearModel
    {
        public int Year { get; set; }
        public int Age { get; set; }
        public decimal RothConversion { get; set; }
        public decimal OtherTaxableIncome { get; set; }
        public decimal TaxableSocialSecurity { get; set; }
        public decimal TaxExemptInterest { get; set; }
        public decimal AdjustedGrossIncome { get; set; } // includes taxable SS and conversion
                                                         // Taxable income after deductions
        public decimal TaxableIncome { get; set; }
        public decimal FederalTaxableIncome { get; set; } // here equal to AGI for simplicity (no itemized/std deduction modeled yet)
        public decimal FederalTax { get; set; }
        public decimal StateTax { get; set; }
        public decimal MAGI_ForIrmaa { get; set; } // AGI + tax-exempt interest
        public string IrmaaTier { get; set; } = "Standard";
       
        // For UI clarity
        public string TaxBracket { get; set; }

        // Optional: Medicare IRMAA surcharge estimate
        public decimal IRMAASurcharge { get; set; }

    }
}
