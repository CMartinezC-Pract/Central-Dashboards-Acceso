using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;
using Microsoft.EntityFrameworkCore;

namespace CentralDashboards.Pages.Incidencias;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IIncidenciaService _svc;
    private readonly CentralDashboardsContext _db;
    public IndexModel(IIncidenciaService svc, CentralDashboardsContext db) { _svc = svc; _db = db; }

    [BindProperty(SupportsGet = true)] public int? EstatusId { get; set; }

    public List<IncidenciaResumenDto> Incidencias { get; set; } = new();
    public List<EstatusDto>           Estatus     { get; set; } = new();

    public async Task OnGetAsync()
    {
        var usuarioId = UserHelper.EsAdmin(User) ? (int?)null : UserHelper.GetUsuarioId(User);
        Incidencias = await _svc.ObtenerTodasAsync(usuarioId: usuarioId, estatusId: EstatusId);
        Estatus = await _db.Estatus
            .Where(e => e.AplicaA == "Incidencia" || e.AplicaA == "Ambos")
            .Select(e => new EstatusDto { EstatusID = e.EstatusID, NombreEstatus = e.NombreEstatus })
            .ToListAsync();
    }
}
