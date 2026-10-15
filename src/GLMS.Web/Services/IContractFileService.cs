using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public interface IContractFileService
{
    void ValidatePdf(string? fileName);
    Task<string> SaveAgreementAsync(IFormFile file, CancellationToken cancellationToken = default);
}
