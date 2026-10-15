using GLMS.Web.Data;
using GLMS.Web.Models;

namespace GLMS.Web.Patterns.Notifications;

public interface IContractObserver
{
    void OnStatusChanged(Contract contract, ContractStatus? previousStatus);
}

public class ContractEventPublisher(IEnumerable<IContractObserver> observers)
{
    public void Publish(Contract contract, ContractStatus? previousStatus)
    {
        foreach (var observer in observers)
        {
            observer.OnStatusChanged(contract, previousStatus);
        }
    }
}

public class FinanceObserver(GlmsDbContext context) : IContractObserver
{
    public void OnStatusChanged(Contract contract, ContractStatus? previousStatus)
    {
        context.ContractNotifications.Add(new ContractNotification
        {
            Contract = contract,
            Department = "Finance",
            Message = $"Contract status: {previousStatus?.ToString() ?? "New"} -> {contract.Status}."
        });
    }
}

public class ComplianceObserver(GlmsDbContext context) : IContractObserver
{
    public void OnStatusChanged(Contract contract, ContractStatus? previousStatus)
    {
        context.ContractNotifications.Add(new ContractNotification
        {
            Contract = contract,
            Department = "Compliance",
            Message = $"Contract status: {previousStatus?.ToString() ?? "New"} -> {contract.Status}."
        });
    }
}
