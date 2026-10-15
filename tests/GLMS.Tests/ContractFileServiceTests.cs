using GLMS.Web.Services;

namespace GLMS.Tests;

public class ContractFileServiceTests
{
    [Theory]
    [InlineData("agreement.pdf")]
    [InlineData("AGREEMENT.PDF")]
    public void ValidatePdf_AcceptsPdfFiles(string fileName)
    {
        var service = CreateService();

        service.ValidatePdf(fileName);
    }

    [Theory]
    [InlineData("agreement.exe")]
    [InlineData("agreement.docx")]
    [InlineData("")]
    public void ValidatePdf_RejectsFilesThatAreNotPdf(string fileName)
    {
        var service = CreateService();

        Assert.Throws<InvalidOperationException>(() => service.ValidatePdf(fileName));
    }

    private static ContractFileService CreateService()
    {
        var environment = new FakeWebHostEnvironment();
        return new ContractFileService(environment);
    }
}
