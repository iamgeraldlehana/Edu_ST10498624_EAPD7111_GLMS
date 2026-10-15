using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers;

public class ContractsController(ContractService contractService, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(ContractStatus? status, DateTime? startDate, DateTime? endDate) =>
        View(await contractService.GetIndexAsync(status, startDate, endDate));

    public async Task<IActionResult> Create()
    {
        await PopulateClientsAsync();
        return View(new Contract());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract,
        IFormFile? signedAgreement, CancellationToken cancellationToken)
    {
        if (signedAgreement is null)
            ModelState.AddModelError(string.Empty, "A signed agreement PDF is required.");
        if (ModelState.IsValid && signedAgreement is not null)
        {
            try
            {
                await contractService.CreateAsync(contract, signedAgreement, cancellationToken);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
            }
        }
        await PopulateClientsAsync(contract.ClientId);
        return View(contract);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, ContractStatus status, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest("Select a valid contract status.");
        try
        {
            if (!await contractService.ChangeStatusAsync(id, status, cancellationToken)) return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadAgreement(int id)
    {
        var contract = await contractService.FindAsync(id);
        if (contract?.SignedAgreementPath is null) return NotFound();

        var path = Path.Combine(environment.WebRootPath, contract.SignedAgreementPath.Replace('/', Path.DirectorySeparatorChar));
        return System.IO.File.Exists(path)
            ? PhysicalFile(path, "application/pdf", $"contract-{contract.Id}-agreement.pdf")
            : NotFound();
    }

    private async Task PopulateClientsAsync(int? selectedClientId = null)
    {
        ViewBag.ClientId = new SelectList(await contractService.GetClientsAsync(), "Id", "Name", selectedClientId);
    }
}
