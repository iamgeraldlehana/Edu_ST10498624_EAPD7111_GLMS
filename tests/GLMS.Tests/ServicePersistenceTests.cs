using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Contracts;
using GLMS.Web.Patterns.Notifications;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests;

public class ServicePersistenceTests
{
    [Fact]
    public async Task Request_SavesServerCalculatedCostInsteadOfPostedCost()
    {
        await using var context = CreateContext();
        var contract = new Contract { ServiceLevel = "Premium", Status = ContractStatus.Active };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        var request = new ServiceRequest
        {
            ContractId = contract.Id, Description = "Transport goods", CostUsd = 100,
            CurrencyRate = 1, CostZar = 1
        };

        await CreateRequestService(context, 18.25m).CreateAsync(request, default);

        var saved = Assert.Single(context.ServiceRequests);
        Assert.Equal(18.25m, saved.CurrencyRate);
        Assert.Equal(1825m, saved.CostZar);
    }

    [Theory]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public async Task BlockedContract_DoesNotSaveRequest(ContractStatus status)
    {
        await using var context = CreateContext();
        var contract = new Contract { ServiceLevel = "Standard", Status = status };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateRequestService(context, 18m).CreateAsync(
                new ServiceRequest { ContractId = contract.Id, Description = "Delivery", CostUsd = 100 }, default));

        Assert.Empty(context.ServiceRequests);
    }

    [Fact]
    public async Task MissingCurrencyRate_DoesNotSaveRequest()
    {
        await using var context = CreateContext();
        var contract = new Contract { ServiceLevel = "Standard", Status = ContractStatus.Active };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateRequestService(context, null).CreateAsync(
                new ServiceRequest { ContractId = contract.Id, Description = "Delivery", CostUsd = 100 }, default));

        Assert.Empty(context.ServiceRequests);
    }

    [Fact]
    public async Task UnknownContract_DoesNotSaveRequest()
    {
        await using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateRequestService(context, 18m).CreateAsync(
                new ServiceRequest { ContractId = 999, Description = "Delivery" }, default));
        Assert.Empty(context.ServiceRequests);
    }

    [Fact]
    public async Task CreateContract_UsesFactoryAndSavesBothDepartmentUpdates()
    {
        await using var context = CreateContext();
        var client = new Client { Name = "Sample Logistics", ContactDetails = "0123456789", Region = "Gauteng" };
        context.Clients.Add(client);
        await context.SaveChangesAsync();
        var service = CreateContractService(context);

        await service.CreateAsync(new Contract
        {
            ClientId = client.Id, ServiceLevel = "Premium", Status = ContractStatus.Active
        }, new FormFile(Stream.Null, 0, 1, "signedAgreement", "agreement.pdf"), default);

        var saved = Assert.Single(context.Contracts);
        Assert.Equal("Premium", saved.ServiceLevel);
        Assert.Equal("uploads/agreements/test.pdf", saved.SignedAgreementPath);
        Assert.Equal(2, context.ContractNotifications.Count());
    }

    [Fact]
    public async Task InvalidContractDates_DoNotSaveContractOrUpdates()
    {
        await using var context = CreateContext();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateContractService(context).CreateAsync(new Contract
            {
                StartDate = new DateTime(2026, 10, 15), EndDate = new DateTime(2026, 10, 1)
            }, new FormFile(Stream.Null, 0, 1, "signedAgreement", "agreement.pdf"), default));
        Assert.Empty(context.Contracts);
        Assert.Empty(context.ContractNotifications);
    }

    [Fact]
    public async Task ContractFilter_CombinesStatusAndDateRange()
    {
        await using var context = CreateContext();
        var client = new Client { Name = "Sample Logistics", ContactDetails = "0123456789", Region = "Gauteng" };
        context.Clients.Add(client);
        await context.SaveChangesAsync();
        context.Contracts.AddRange(
            new Contract { ClientId = client.Id, ServiceLevel = "Standard", Status = ContractStatus.Active, StartDate = new(2026, 10, 1), EndDate = new(2026, 10, 31) },
            new Contract { ClientId = client.Id, ServiceLevel = "Standard", Status = ContractStatus.Expired, StartDate = new(2026, 10, 1), EndDate = new(2026, 10, 31) },
            new Contract { ClientId = client.Id, ServiceLevel = "Standard", Status = ContractStatus.Active, StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31) });
        await context.SaveChangesAsync();

        var result = await CreateContractService(context).GetIndexAsync(
            ContractStatus.Active, new DateTime(2026, 10, 1), new DateTime(2026, 10, 31));

        Assert.Equal(ContractStatus.Active, Assert.Single(result.Contracts).Status);
    }

    private static GlmsDbContext CreateContext() => new(
        new DbContextOptionsBuilder<GlmsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static IContractFactory[] Factories() =>
        [new StandardContractFactory(new CurrencyCalculator()), new PremiumContractFactory(new CurrencyCalculator())];

    private static ServiceRequestService CreateRequestService(GlmsDbContext context, decimal? rate) =>
        new(context, Factories(), new FixedRateService(rate), new ServiceRequestWorkflow());

    private static ContractService CreateContractService(GlmsDbContext context) =>
        new(context, Factories(), new SavedFileService(),
            new ContractEventPublisher([new FinanceObserver(context), new ComplianceObserver(context)]));

    private sealed class FixedRateService(decimal? rate) : ICurrencyRateService
    {
        public Task<decimal?> GetUsdToZarRateAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(rate);
    }

    private sealed class SavedFileService : IContractFileService
    {
        public void ValidatePdf(string? fileName) { }
        public Task<string> SaveAgreementAsync(IFormFile file, CancellationToken cancellationToken = default) =>
            Task.FromResult("uploads/agreements/test.pdf");
    }
}
