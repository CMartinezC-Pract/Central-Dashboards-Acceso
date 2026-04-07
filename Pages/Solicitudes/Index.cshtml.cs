using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos; // Priorizamos este namespace
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Solicitudes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ISolicitudService _svc;
    private readonly CentralDashboards.Data.CentralDashboardsContext _db; // Ruta completa para evitar conflicto

    public IndexModel(ISolicitudService svc, CentralDashboards.Data.CentralDashboardsContext db)
    {
        _svc = svc;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int? EstatusId { get; set; }

    // Estas ahora apuntan sin duda a CentralDashboards.Models.Dtos
    public List<SolicitudResumenDto> Solicituds { get; set; } = new();
    public List<EstatusDto> Estatus { get; set; } = new();

    public async Task OnGetAsync()
    {
        var usuarioId = UserHelper.EsAdmin(User) ? (int?)null : UserHelper.GetUsuarioId(User);

        Solicituds = await _svc.ObtenerTodasAsync(usuarioId: usuarioId, estatusId: EstatusId);

        Estatus = await _db.Estatus
            .Where(e => e.AplicaA == "Solicitud" || e.AplicaA == "Ambos")
            .Select(e => new EstatusDto
            {
                EstatusID = e.EstatusID,
                NombreEstatus = e.NombreEstatus
            })
            .ToListAsync();
    }
}