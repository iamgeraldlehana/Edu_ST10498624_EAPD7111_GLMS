using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Tests;

public class ServiceRequestWorkflowTests
{
    private readonly ServiceRequestWorkflow _workflow = new();

    [Theory]
    [InlineData(ContractStatus.Draft)]
    [InlineData(ContractStatus.Active)]
    public void CanCreateRequest_AllowsDraftAndActiveContracts(ContractStatus status)
    {
        Assert.True(_workflow.CanCreateRequest(status));
    }

    [Theory]
    [InlineData(ContractStatus.Expired)]
    [InlineData(ContractStatus.OnHold)]
    public void CanCreateRequest_BlocksExpiredAndOnHoldContracts(ContractStatus status)
    {
        Assert.False(_workflow.CanCreateRequest(status));
    }
}
