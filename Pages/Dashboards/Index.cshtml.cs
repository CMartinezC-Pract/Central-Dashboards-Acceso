using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Dashboards;

[Authorize(Policy = "SoloAdmin")]
[IgnoreAntiforgeryToken]  // ← agregar

public class IndexModel : PageModel
{
    private readonly IDashboardService _svc;
    private readonly IUnidadNegocioService _unidades;

    public IndexModel(IDashboardService svc, IUnidadNegocioService unidades)
    {
        _svc = svc;
        _unidades = unidades;
    }

    [BindProperty(SupportsGet = true)] public int? UnidadId { get; set; }
    [BindProperty(SupportsGet = true)] public bool SoloActivos { get; set; } = false;


    public List<DashboardDto> Dashboards { get; set; } = new();
    public List<UnidadNegocioDto> Unidades { get; set; } = new();

    public async Task OnGetAsync()
    {
        Unidades = await _unidades.ObtenerTodasAsync(soloActivas: false);
        Dashboards = await _svc.ObtenerTodosAsync(UnidadId, SoloActivos);
    }

    public async Task<IActionResult> OnPostDesactivarAsync(int id)
    {
        await _svc.DesactivarAsync(id);
        TempData["Exito"] = "Dashboard desactivado correctamente.";
        return RedirectToPage();
    }
}