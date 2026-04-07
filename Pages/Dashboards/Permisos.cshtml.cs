using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Dashboards;

[Authorize(Policy = "SoloAdmin")]
public class PermisosModel : PageModel
{
    private readonly IDashboardService _svc;
    private readonly IUsuarioService _usuarios;
    private readonly IAuditoriaService _audit;
    private readonly INotificacionService _notif;

    public PermisosModel(IDashboardService svc, IUsuarioService usuarios,
                         IAuditoriaService audit, INotificacionService notif)
    {
        _svc = svc;
        _usuarios = usuarios;
        _audit = audit;
        _notif = notif;
    }

    public int DashboardId { get; set; }
    public string NombreDashboard { get; set; } = "";
    public string Folio { get; set; } = "";
    public List<DashboardPermisoDto> Permisos { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var d = await _svc.ObtenerPorIdAsync(id);
        if (d == null) return NotFound();

        DashboardId = id;
        NombreDashboard = d.Nombre;
        Folio = d.Folio;
        Permisos = await _svc.ObtenerPermisosAsync(id);
        return Page();
    }

    // ── AJAX: buscar usuario por nombre o correo ──────────────
    public async Task<IActionResult> OnGetBuscarUsuarioAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return new JsonResult(new { encontrado = false, mensaje = "Escribe un nombre o correo." });

        var todos = await _usuarios.ObtenerTodosAsync(soloActivos: true);
        var t = termino.Trim().ToLower();

        var usuario = todos.FirstOrDefault(u =>
            u.Correo.ToLower().Contains(t) ||
            u.Nombre.ToLower().Contains(t));

        if (usuario == null)
            return new JsonResult(new { encontrado = false, mensaje = "No se encontró ningún usuario activo con ese nombre o correo." });

        return new JsonResult(new
        {
            encontrado = true,
            usuarioId = usuario.UsuarioID,
            nombre = usuario.Nombre,
            correo = usuario.Correo,
            nombreRol = usuario.NombreRol
        });
    }

    // ── AJAX: otorgar permiso ─────────────────────────────────
    public async Task<IActionResult> OnPostOtorgarPermisoAsync([FromBody] PermisoRequest req)
    {
        try
        {
            var adminId = UserHelper.GetUsuarioId(User);
            await _svc.OtorgarPermisoAsync(req.DashboardId, req.UsuarioId, adminId);

            var admin = await _usuarios.ObtenerPorIdAsync(adminId);
            var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
            var dashboard = await _svc.ObtenerPorIdAsync(req.DashboardId);

            await _audit.RegistrarAsync(adminId, "OtorgarPermiso", "DashboardPermisos",
                req.DashboardId, $"UsuarioID:{req.UsuarioId}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Notificar al usuario que recibió acceso
            if (dashboard != null)
                await _notif.NotificarPermisoOtorgadoAsync(
                    req.UsuarioId, dashboard.Nombre, req.DashboardId);

            return new JsonResult(new
            {
                success = true,
                permiso = new
                {
                    usuarioID = usuario?.UsuarioID,
                    nombre = usuario?.Nombre,
                    correo = usuario?.Correo,
                    nombreRol = usuario?.NombreRol,
                    otorgadoPor = admin?.Nombre
                }
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    // ── AJAX: revocar permiso ─────────────────────────────────
    public async Task<IActionResult> OnPostRevocarPermisoAsync([FromBody] PermisoRequest req)
    {
        try
        {
            var adminId = UserHelper.GetUsuarioId(User);
            var dashboard = await _svc.ObtenerPorIdAsync(req.DashboardId);

            await _svc.RevocarPermisoAsync(req.DashboardId, req.UsuarioId);

            await _audit.RegistrarAsync(adminId, "RevocarPermiso", "DashboardPermisos",
                req.DashboardId, $"UsuarioID:{req.UsuarioId}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            // Notificar al usuario que perdió el acceso
            if (dashboard != null)
                await _notif.NotificarPermisoRevocadoAsync(req.UsuarioId, dashboard.Nombre);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}

public class PermisoRequest
{
    public int DashboardId { get; set; }
    public int UsuarioId { get; set; }
}