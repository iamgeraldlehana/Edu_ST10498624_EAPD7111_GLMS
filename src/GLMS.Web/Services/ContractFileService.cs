using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public class ContractFileService(IWebHostEnvironment environment) : IContractFileService
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public void ValidatePdf(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only PDF files can be uploaded.");
        }
    }

    public async Task<string> SaveAgreementAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidatePdf(file.FileName);
        if (file.Length == 0 || file.Length > MaxFileSize)
        {
            throw new InvalidOperationException("The PDF must be between 1 byte and 5 MB.");
        }

        var folder = Path.Combine(environment.WebRootPath, "uploads", "agreements");
        Directory.CreateDirectory(folder);
        var savedName = $"{Guid.NewGuid():N}.pdf";
        var path = Path.Combine(folder, savedName);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);
        return Path.Combine("uploads", "agreements", savedName).Replace("\\", "/");
    }
}
