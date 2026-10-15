using System.Net;
using System.Text;
using GLMS.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace GLMS.Tests;

public class CurrencyApiAdapterTests
{
    [Fact]
    public async Task LowerCaseApiResponse_IsConvertedToExchangeRate()
    {
        var adapter = CreateAdapter(HttpStatusCode.OK, """{"base":"USD","rates":{"ZAR":18.25}}""");

        var rate = await adapter.GetExchangeRateAsync();

        Assert.Equal(new ExchangeRate("USD", "ZAR", 18.25m), rate);
        Assert.Equal(18.25m, await adapter.GetUsdToZarRateAsync());
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "{}")]
    [InlineData(HttpStatusCode.OK, "not json")]
    [InlineData(HttpStatusCode.OK, """{"rates":{"EUR":0.9}}""")]
    [InlineData(HttpStatusCode.OK, """{"rates":{"ZAR":0}}""")]
    [InlineData(HttpStatusCode.OK, """{"rates":{"ZAR":-1}}""")]
    public async Task UnavailableOrInvalidResponse_DoesNotReturnUsableRate(HttpStatusCode status, string body)
    {
        Assert.Null(await CreateAdapter(status, body).GetUsdToZarRateAsync());
    }

    private static CurrencyApiAdapter CreateAdapter(HttpStatusCode status, string body) => new(
        new CurrencyApiClient(new HttpClient(new ApiHandler(status, body))),
        NullLogger<CurrencyApiAdapter>.Instance);

    private sealed class ApiHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
    }
}
