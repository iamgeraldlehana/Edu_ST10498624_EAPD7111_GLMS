using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Contracts;
using GLMS.Web.Patterns.Notifications;
using GLMS.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Tests;

public class ContractPatternTests
{
    [Theory]
    [InlineData("Standard", typeof(StandardPricingRule))]
    [InlineData("Premium", typeof(PremiumPricingRule))]
    public void Factory_CreatesMatchingContractAndPricingRule(string level, Type ruleType)
    {
        IContractFactory factory = level == "Standard"
            ? new StandardContractFactory(new CurrencyCalculator())
            : new PremiumContractFactory(new CurrencyCalculator());

        Assert.Equal(level, factory.CreateContract().ServiceLevel);
        Assert.IsType(ruleType, factory.CreatePricingRule());
        Assert.Equal(1825m, factory.CreatePricingRule().CalculateZar(100m, 18.25m));
    }

    [Fact]
    public async Task StatusChange_SavesUpdatesForFinanceAndCompliance()
    {
        await using var context = CreateContext();
        var contract = new Contract { Status = ContractStatus.Draft, ServiceLevel = "Standard" };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        Assert.True(await service.ChangeStatusAsync(contract.Id, ContractStatus.Active, default));

        Assert.Equal(ContractStatus.Active, contract.Status);
        var updates = await context.ContractNotifications.OrderBy(update => update.Department).ToListAsync();
        Assert.Equal(new[] { "Compliance", "Finance" }, updates.Select(update => update.Department));
        Assert.All(updates, update =>
        {
            Assert.Equal(contract.Id, update.ContractId);
            Assert.Contains("Draft -> Active", update.Message);
        });
    }

    [Fact]
    public async Task UnchangedStatus_DoesNotSendDuplicateUpdates()
    {
        await using var context = CreateContext();
        var contract = new Contract { Status = ContractStatus.Active, ServiceLevel = "Standard" };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        await CreateService(context).ChangeStatusAsync(contract.Id, ContractStatus.Active, default);

        Assert.Empty(context.ContractNotifications);
    }

    [Fact]
    public async Task InvalidStatus_DoesNotChangeContract()
    {
        await using var context = CreateContext();
        var contract = new Contract { Status = ContractStatus.Active, ServiceLevel = "Standard" };
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateService(context).ChangeStatusAsync(contract.Id, (ContractStatus)99, default));

        Assert.Equal(ContractStatus.Active, contract.Status);
        Assert.Empty(context.ContractNotifications);
    }

    [Fact]
    public async Task UnknownContract_ReturnsFalse()
    {
        await using var context = CreateContext();
        Assert.False(await CreateService(context).ChangeStatusAsync(999, ContractStatus.Active, default));
        Assert.Empty(context.ContractNotifications);
    }

    private static GlmsDbContext CreateContext() => new(
        new DbContextOptionsBuilder<GlmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ContractService CreateService(GlmsDbContext context) => new(
        context,
        [new StandardContractFactory(new CurrencyCalculator()), new PremiumContractFactory(new CurrencyCalculator())],
        new ContractFileService(new FakeWebHostEnvironment()),
        new ContractEventPublisher([new FinanceObserver(context), new ComplianceObserver(context)]));
}
