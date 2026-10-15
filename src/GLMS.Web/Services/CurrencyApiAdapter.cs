using System.Text.Json;

namespace GLMS.Web.Services;

public record ExchangeRate(string From, string To, decimal Rate);

public class CurrencyApiAdapter(CurrencyApiClient client, ILogger<CurrencyApiAdapter> logger) : ICurrencyRateService
{
    public async Task<decimal?> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        var exchangeRate = await GetExchangeRateAsync(cancellationToken);
        return exchangeRate?.Rate;
    }

    public async Task<ExchangeRate?> GetExchangeRateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await client.GetLatestAsync(cancellationToken);
            return result?.Rates?.TryGetValue("ZAR", out var rate) == true && rate > 0
                ? new ExchangeRate("USD", "ZAR", rate)
                : null;
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException ||
            exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "The currency rate is currently unavailable.");
            return null;
        }
    }
}
