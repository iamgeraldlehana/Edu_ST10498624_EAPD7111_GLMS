namespace GLMS.Web.Services;

public interface ICurrencyCalculator
{
    decimal CalculateZar(decimal usdAmount, decimal usdToZarRate);
}
