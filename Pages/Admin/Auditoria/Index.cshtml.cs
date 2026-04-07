using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.Auditoria;

[Authorize(Policy = "SoloAdmin")]
public class IndexModel : PageModel
{
    private readonly IAuditoriaService _svc;
    public IndexModel(IAuditoriaService svc) => _svc = svc;
    public List<AuditoriaDto> Registros { get; set; } = new();

    public async Task OnGetAsync() => Registros = await _svc.ObtenerRecentesAsync(200);
}
