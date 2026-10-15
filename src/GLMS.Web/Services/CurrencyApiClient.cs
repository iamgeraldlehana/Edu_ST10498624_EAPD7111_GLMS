using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GLMS.Web.Services;

public class CurrencyApiClient(HttpClient httpClient)
{
    public Task<CurrencyApiResponse?> GetLatestAsync(CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<CurrencyApiResponse>(
            "https://api.frankfurter.app/latest?from=USD&to=ZAR", cancellationToken);
}

public class CurrencyApiResponse
{
    [JsonPropertyName("rates")]
    public Dictionary<string, decimal>? Rates { get; init; }
}
