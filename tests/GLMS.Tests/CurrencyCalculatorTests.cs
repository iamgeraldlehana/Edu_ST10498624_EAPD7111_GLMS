using GLMS.Web.Services;

namespace GLMS.Tests;

public class CurrencyCalculatorTests
{
    private readonly CurrencyCalculator _calculator = new();

    [Fact]
    public void CalculateZar_UsesTheGivenRate()
    {
        var result = _calculator.CalculateZar(100m, 18.25m);

        Assert.Equal(1825m, result);
    }

    [Fact]
    public void CalculateZar_RoundsToTwoDecimalPlaces()
    {
        var result = _calculator.CalculateZar(12.345m, 18.9876m);

        Assert.Equal(234.4m, result);
    }

    [Fact]
    public void CalculateZar_RejectsNegativeAmounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.CalculateZar(-1m, 18m));
    }

    [Fact]
    public void CalculateZar_RejectsZeroRates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calculator.CalculateZar(10m, 0m));
    }
}
