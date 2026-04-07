using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.UnidadesNegocio;

[Authorize(Policy = "SoloAdmin")]
public class IndexModel : PageModel
{
    private readonly IUnidadNegocioService _svc;
    public IndexModel(IUnidadNegocioService svc) => _svc = svc;

    public List<UnidadNegocioDto> Unidades { get; set; } = new();

    public async Task OnGetAsync()
        => Unidades = await _svc.ObtenerTodasAsync(soloActivas: false);
}