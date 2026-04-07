using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Solicitudes;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly ISolicitudService _svc;
    private readonly IAuditoriaService _audit;
    private readonly CentralDashboardsContext _db;

    public EditModel(ISolicitudService svc, IAuditoriaService audit, CentralDashboardsContext db)
    {
        _svc = svc;
        _audit = audit;
        _db = db;
    }

    [BindProperty]
    public SolicitudEditDto Input { get; set; } = new();

    public string Folio { get; set; } = "";
    public SelectList Estatus { get; set; } = default!;
    public SelectList Tipos { get; set; } = default!;
    public SelectList Admins { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inc = await _svc.ObtenerPorIdAsync(id);
        if (inc == null) return NotFound();

        Folio = inc.Folio;
        Input = new SolicitudEditDto
        {
            SolicitudID = inc.SolicitudID,
            Titulo = inc.Titulo,
            Descripcion = inc.Descripcion,
            TipoSolicitudID = inc.TipoSolicitudID,
            EstatusID = inc.EstatusID,
            AsignadoAID = inc.AsignadoAID,
            Prioridad = inc.Prioridad
        };

        await CargarListasAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return Page();
        }

        var uid = UserHelper.GetUsuarioId(User);

        await _svc.ActualizarAsync(Input);
        await _svc.CambiarEstatusAsync(Input.SolicitudID, Input.EstatusID, Input.AsignadoAID);
        await _audit.RegistrarAsync(
            uid,
            "EditarSolicitud",
            "Solicitudes",
            Input.SolicitudID,
            $"Estatus:{Input.EstatusID}",
            HttpContext.Connection.RemoteIpAddress?.ToString()
        );

        TempData["Exito"] = "Solicitud actualizada correctamente.";
        return RedirectToPage("./Detail", new { id = Input.SolicitudID });
    }

    private async Task CargarListasAsync()
    {
        var listaEstatus = await _db.Estatus
            .Where(e => e.AplicaA == "Solicitud" || e.AplicaA == "Ambos")
            .ToListAsync();
        Estatus = new SelectList(listaEstatus, "EstatusID", "NombreEstatus");

        var listaTipos = await _db.TiposSolicitud.ToListAsync();
        Tipos = new SelectList(listaTipos, "TipoID", "Nombre");

        var listaAdmins = await _db.Usuarios
            .Join(_db.Roles, u => u.RolID, r => r.RolID, (u, r) => new { u, r })
            .Where(x => x.r.NombreRol == "Administrador" && x.u.Activo)
            .Select(x => new { x.u.UsuarioID, x.u.Nombre })
            .ToListAsync();
        Admins = new SelectList(listaAdmins, "UsuarioID", "Nombre");
    }
}