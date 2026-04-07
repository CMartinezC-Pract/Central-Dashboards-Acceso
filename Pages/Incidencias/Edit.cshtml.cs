using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Incidencias;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly IIncidenciaService _svc;
    private readonly IAuditoriaService  _audit;
    private readonly CentralDashboardsContext _db;

    public EditModel(IIncidenciaService svc, IAuditoriaService audit, CentralDashboardsContext db)
    { _svc = svc; _audit = audit; _db = db; }

    [BindProperty] public IncidenciaEditDto Input { get; set; } = new();
    public string       Folio   { get; set; } = "";
    public SelectList   Estatus { get; set; } = default!;
    public SelectList   Tipos   { get; set; } = default!;
    public SelectList   Admins  { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inc = await _svc.ObtenerPorIdAsync(id);
        if (inc == null) return NotFound();

        Folio = inc.Folio;
        Input = new IncidenciaEditDto
        {
            IncidenciaID     = inc.IncidenciaID,
            Titulo           = inc.Titulo,
            Descripcion      = inc.Descripcion,
            Prioridad        = inc.Prioridad,
            TipoIncidenciaID = inc.TipoIncidenciaID,
            EstatusID        = inc.EstatusID,
            AsignadoAID      = inc.AsignadoAID
        };
        await CargarListasAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CargarListasAsync(); return Page(); }

        var uid = UserHelper.GetUsuarioId(User);
        await _svc.ActualizarAsync(Input);
        await _svc.CambiarEstatusAsync(Input.IncidenciaID, Input.EstatusID, Input.AsignadoAID);
        await _audit.RegistrarAsync(uid, "EditarIncidencia", "Incidencias", Input.IncidenciaID,
            $"Estatus:{Input.EstatusID}", HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = "Incidencia actualizada correctamente.";
        return RedirectToPage("./Detail", new { id = Input.IncidenciaID });
    }

    private async Task CargarListasAsync()
    {
        Estatus = new SelectList(await _db.Estatus
            .Where(e => e.AplicaA == "Incidencia" || e.AplicaA == "Ambos")
            .ToListAsync(), "EstatusID", "NombreEstatus");

        Tipos = new SelectList(await _db.TiposIncidencia.ToListAsync(), "TipoID", "Nombre");

        Admins = new SelectList(await _db.Usuarios
            .Join(_db.Roles, u => u.RolID, r => r.RolID, (u, r) => new { u, r })
            .Where(x => x.r.NombreRol == "Administrador" && x.u.Activo)
            .Select(x => new { x.u.UsuarioID, x.u.Nombre })
            .ToListAsync(), "UsuarioID", "Nombre");
    }
}
