using GLMS.Web.Models;

namespace GLMS.Web.Services;

public interface IServiceRequestWorkflow
{
    bool CanCreateRequest(ContractStatus contractStatus);
}
