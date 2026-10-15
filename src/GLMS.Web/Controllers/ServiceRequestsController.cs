using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers;

public class ServiceRequestsController(
    ServiceRequestService requestService,
    ICurrencyRateService currencyRateService) : Controller
{
    public async Task<IActionResult> Index() => View(await requestService.GetAllAsync());

    public async Task<IActionResult> Create()
    {
        await PopulateContractsAsync();
        return View(new ServiceRequest());
    }

    [HttpGet]
    public async Task<IActionResult> GetUsdToZarRate(CancellationToken cancellationToken)
    {
        var rate = await currencyRateService.GetUsdToZarRateAsync(cancellationToken);
        return rate.HasValue ? Json(new { rate }) : StatusCode(503, new { message = "Currency rate is currently unavailable." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ContractId,Description,CostUsd,Status")] ServiceRequest request,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await requestService.CreateAsync(request, cancellationToken);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }
        await PopulateContractsAsync(request.ContractId);
        return View(request);
    }

    private async Task PopulateContractsAsync(int? selectedContractId = null)
    {
        var contracts = await requestService.GetContractsAsync();
        ViewBag.ContractId = new SelectList(contracts.Select(contract => new
        {
            contract.Id,
            Name = $"#{contract.Id} - {contract.Client!.Name} - {contract.ServiceLevel} ({contract.Status})"
        }), "Id", "Name", selectedContractId);
    }
}
