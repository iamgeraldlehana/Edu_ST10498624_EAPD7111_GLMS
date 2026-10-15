using GLMS.Web.Services;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers;

public class ClientsController(ClientService clientService) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await clientService.GetAllAsync());
    }

    public IActionResult Create()
    {
        return View(new Client());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,ContactDetails,Region")] Client client)
    {
        if (!ModelState.IsValid)
        {
            return View(client);
        }

        await clientService.CreateAsync(client);
        return RedirectToAction(nameof(Index));
    }
}
