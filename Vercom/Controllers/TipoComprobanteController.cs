using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VERCOM.Services;

namespace VERCOM.Controllers;

[Authorize]
public class TipoComprobanteController : Controller
{
    private readonly IAccountingService _accountingService;

    public TipoComprobanteController(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    public async Task<IActionResult> Index()
    {
        var types = await _accountingService.GetVoucherTypesAsync();
        return View(types);
    }
}
