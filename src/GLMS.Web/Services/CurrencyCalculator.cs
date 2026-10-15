namespace GLMS.Web.Services;

public class CurrencyCalculator : ICurrencyCalculator
{
    public decimal CalculateZar(decimal usdAmount, decimal usdToZarRate)
    {
        if (usdAmount < 0 || usdToZarRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(usdAmount), "Amount and exchange rate must be valid.");
        }

        return decimal.Round(usdAmount * usdToZarRate, 2, MidpointRounding.AwayFromZero);
    }
}
