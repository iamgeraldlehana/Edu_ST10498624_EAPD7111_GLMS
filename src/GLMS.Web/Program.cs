using GLMS.Web.Data;
using GLMS.Web.Services;
using GLMS.Web.Patterns.Contracts;
using GLMS.Web.Patterns.Notifications;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<GlmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GlmsDatabase")));
builder.Services.AddScoped<ICurrencyCalculator, CurrencyCalculator>();
builder.Services.AddScoped<IContractFileService, ContractFileService>();
builder.Services.AddScoped<IServiceRequestWorkflow, ServiceRequestWorkflow>();
builder.Services.AddScoped<IContractFactory, StandardContractFactory>();
builder.Services.AddScoped<IContractFactory, PremiumContractFactory>();
builder.Services.AddScoped<IContractObserver, FinanceObserver>();
builder.Services.AddScoped<IContractObserver, ComplianceObserver>();
builder.Services.AddScoped<ContractEventPublisher>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ServiceRequestService>();
builder.Services.AddHttpClient<CurrencyApiClient>();
builder.Services.AddScoped<ICurrencyRateService, CurrencyApiAdapter>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
