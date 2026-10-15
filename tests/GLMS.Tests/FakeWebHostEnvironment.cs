using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace GLMS.Tests;

public class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "GLMS.Tests";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Testing";
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
