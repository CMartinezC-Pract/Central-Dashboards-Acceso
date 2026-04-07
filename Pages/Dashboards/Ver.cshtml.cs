using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Dashboards;

[Authorize]
public class VerModel : PageModel
{
    private readonly IDashboardService _dash;
    private readonly CentralDashboardsContext _db;

    public VerModel(IDashboardService dash, CentralDashboardsContext db)
    {
        _dash = dash;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int DashboardId { get; set; }

    public DashboardDto? Dashboard { get; set; }
    public bool TieneAcceso { get; set; }
    public List<TipoDto> TiposIncidencia { get; set; } = new();
    public List<TipoDto> TiposSolicitud { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // ── Anti-caché ────────────────────────────────────────────
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        if (DashboardId <= 0)
            return RedirectToPage("/Index");

        var uid = UserHelper.GetUsuarioId(User);
        Dashboard = await _dash.ObtenerPorIdAsync(DashboardId);
        if (Dashboard == null)
            return RedirectToPage("/Index");

        TieneAcceso = await _dash.VerificarAccesoAsync(DashboardId, uid);

        TiposIncidencia = _db.TiposIncidencia
            .Select(t => new TipoDto { TipoID = t.TipoID, Nombre = t.Nombre })
            .ToList();

        TiposSolicitud = _db.TiposSolicitud
            .Select(t => new TipoDto { TipoID = t.TipoID, Nombre = t.Nombre })
            .ToList();

        return Page();
    }

    /// <summary>
    /// GET /Dashboards/Ver?handler=Documento&DashboardId=10
    /// Sin ?download → inline (se muestra en iframe).
    /// Con ?download   → attachment (se descarga).
    /// </summary>
    public async Task<IActionResult> OnGetDocumentoAsync()
    {
        if (DashboardId <= 0) return NotFound();

        var entity = await _db.Dashboards.FindAsync(DashboardId);
        if (entity?.DocumentacionData == null) return NotFound();

        var tipo = entity.DocumentacionTipo ?? "application/octet-stream";
        var nombre = entity.DocumentacionNombre ?? "documento";

        bool descargar = HttpContext.Request.Query.ContainsKey("download");
        return descargar
            ? File(entity.DocumentacionData, tipo, nombre)  // attachment → descarga
            : File(entity.DocumentacionData, tipo);          // inline → se muestra en iframe
    }
}