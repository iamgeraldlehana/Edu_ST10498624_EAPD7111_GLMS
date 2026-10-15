using GLMS.Web.Models;

namespace GLMS.Web.ViewModels;

public class ContractIndexViewModel
{
    public ContractStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Contract> Contracts { get; set; } = [];
    public List<ContractNotification> Notifications { get; set; } = [];
}
