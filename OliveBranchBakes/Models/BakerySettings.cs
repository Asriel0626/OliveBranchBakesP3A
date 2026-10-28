namespace OliveBranchBakes.Models;

/// <summary>Stores bakery settings and the selected rule labels.</summary>
public sealed class BakerySettings
{
    // Default selection for this bakery variant.
    public string VariantDigits { get; set; } = "319";
    public bool IsDemoVariant { get; set; } = false;
    public decimal TaxRate { get; set; } = 0.085m;
    public int MinimumAdvanceHours { get; set; } = 24;
    public int MaximumAdvanceDays { get; set; } = 30;
    public string SeasonalItemLabel => VariantRules.SeasonalName(VariantDigits[0]);
    public string OperationsLabel => VariantRules.OperationsLabel(VariantDigits[1]);
    public string PricingLabel => VariantRules.PricingLabel(VariantDigits[2]);
}
