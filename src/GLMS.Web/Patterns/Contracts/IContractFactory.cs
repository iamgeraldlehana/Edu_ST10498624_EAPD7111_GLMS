using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Patterns.Contracts;

public interface IContractFactory
{
    string ServiceLevel { get; }
    Contract CreateContract();
    IContractPricingRule CreatePricingRule();
}

public interface IContractPricingRule
{
    decimal CalculateZar(decimal usdAmount, decimal rate);
}

public class StandardContractFactory(ICurrencyCalculator calculator) : IContractFactory
{
    public string ServiceLevel => "Standard";
    public Contract CreateContract() => new() { ServiceLevel = ServiceLevel };
    public IContractPricingRule CreatePricingRule() => new StandardPricingRule(calculator);
}

public class PremiumContractFactory(ICurrencyCalculator calculator) : IContractFactory
{
    public string ServiceLevel => "Premium";
    public Contract CreateContract() => new() { ServiceLevel = ServiceLevel };
    public IContractPricingRule CreatePricingRule() => new PremiumPricingRule(calculator);
}

public class StandardPricingRule(ICurrencyCalculator calculator) : IContractPricingRule
{
    public decimal CalculateZar(decimal usdAmount, decimal rate) => calculator.CalculateZar(usdAmount, rate);
}

public class PremiumPricingRule(ICurrencyCalculator calculator) : IContractPricingRule
{
    // Both service levels currently use the same currency calculation.
    public decimal CalculateZar(decimal usdAmount, decimal rate) => calculator.CalculateZar(usdAmount, rate);
}
