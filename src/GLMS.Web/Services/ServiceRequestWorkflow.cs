using GLMS.Web.Models;

namespace GLMS.Web.Services;

public class ServiceRequestWorkflow : IServiceRequestWorkflow
{
    public bool CanCreateRequest(ContractStatus contractStatus)
    {
        return contractStatus is ContractStatus.Draft or ContractStatus.Active;
    }
}
