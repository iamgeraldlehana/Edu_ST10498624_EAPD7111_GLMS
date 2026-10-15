using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Contracts;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Services;

public class ServiceRequestService(
    GlmsDbContext context,
    IEnumerable<IContractFactory> factories,
    ICurrencyRateService currencyRateService,
    IServiceRequestWorkflow workflow)
{
    public Task<List<ServiceRequest>> GetAllAsync() =>
        context.ServiceRequests.Include(request => request.Contract)
            .ThenInclude(contract => contract!.Client)
            .OrderByDescending(request => request.Id).ToListAsync();

    public Task<List<Contract>> GetContractsAsync() =>
        context.Contracts.Include(contract => contract.Client)
            .OrderBy(contract => contract.Client!.Name).ToListAsync();

    public async Task CreateAsync(ServiceRequest request, CancellationToken cancellationToken)
    {
        var contract = await context.Contracts.FindAsync([request.ContractId], cancellationToken)
            ?? throw new InvalidOperationException("Select a valid contract.");
        if (!workflow.CanCreateRequest(contract.Status))
            throw new InvalidOperationException("A request cannot be created for an expired or on-hold contract.");
        if (!Enum.IsDefined(request.Status))
            throw new InvalidOperationException("Select a valid request status.");
        if (request.CostUsd < 0 || request.CostUsd > 100000000)
            throw new InvalidOperationException("Enter a valid USD cost.");

        var factory = factories.FirstOrDefault(factory => factory.ServiceLevel == contract.ServiceLevel)
            ?? throw new InvalidOperationException("The contract must have a Standard or Premium service level.");
        var rate = await currencyRateService.GetUsdToZarRateAsync(cancellationToken)
            ?? throw new InvalidOperationException("The currency rate is not available. Try again later.");

        request.CurrencyRate = rate;
        request.CostZar = factory.CreatePricingRule().CalculateZar(request.CostUsd, rate);
        context.ServiceRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);
    }
}
