namespace GLMS.Web.Services;

public interface ICurrencyRateService
{
    Task<decimal?> GetUsdToZarRateAsync(CancellationToken cancellationToken = default);
}
