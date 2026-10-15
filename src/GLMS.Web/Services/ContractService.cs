using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Patterns.Contracts;
using GLMS.Web.Patterns.Notifications;
using GLMS.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Services;

public class ContractService(
    GlmsDbContext context,
    IEnumerable<IContractFactory> factories,
    IContractFileService fileService,
    ContractEventPublisher publisher)
{
    public async Task<ContractIndexViewModel> GetIndexAsync(ContractStatus? status, DateTime? startDate, DateTime? endDate)
    {
        var contracts = context.Contracts.Include(contract => contract.Client).AsQueryable();
        if (status.HasValue) contracts = contracts.Where(contract => contract.Status == status.Value);
        if (startDate.HasValue) contracts = contracts.Where(contract => contract.StartDate >= startDate.Value);
        if (endDate.HasValue) contracts = contracts.Where(contract => contract.EndDate <= endDate.Value);
        return new ContractIndexViewModel
        {
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            Contracts = await contracts.OrderBy(contract => contract.EndDate).ToListAsync(),
            Notifications = await context.ContractNotifications.OrderByDescending(notification => notification.Id).Take(20).ToListAsync()
        };
    }

    public Task<List<Client>> GetClientsAsync() =>
        context.Clients.OrderBy(client => client.Name).ToListAsync();

    public ValueTask<Contract?> FindAsync(int id) => context.Contracts.FindAsync(id);

    public async Task CreateAsync(Contract input, IFormFile agreement, CancellationToken cancellationToken)
    {
        if (input.EndDate < input.StartDate)
            throw new InvalidOperationException("End date cannot be before the start date.");
        if (!Enum.IsDefined(input.Status))
            throw new InvalidOperationException("Select a valid contract status.");
        if (!await context.Clients.AnyAsync(client => client.Id == input.ClientId, cancellationToken))
            throw new InvalidOperationException("Select a valid client.");

        var factory = factories.FirstOrDefault(factory => factory.ServiceLevel == input.ServiceLevel)
            ?? throw new InvalidOperationException("Select Standard or Premium.");
        var contract = factory.CreateContract();
        contract.ClientId = input.ClientId;
        contract.StartDate = input.StartDate;
        contract.EndDate = input.EndDate;
        contract.Status = input.Status;
        contract.SignedAgreementPath = await fileService.SaveAgreementAsync(agreement, cancellationToken);

        context.Contracts.Add(contract);
        publisher.Publish(contract, null);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ChangeStatusAsync(int id, ContractStatus status, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(status))
            throw new InvalidOperationException("Select a valid contract status.");
        var contract = await context.Contracts.FindAsync([id], cancellationToken);
        if (contract is null) return false;
        if (contract.Status == status) return true;

        var previousStatus = contract.Status;
        contract.Status = status;
        publisher.Publish(contract, previousStatus);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
