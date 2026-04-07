using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CentralDashboards.Pages.Solicitudes;

[Authorize]
public class DetailModel : PageModel
{
    private readonly ISolicitudService _svc;
    private readonly IComentarioService _com;
    private readonly IAuditoriaService _audit;
    private readonly INotificacionService _notif;
    private readonly IUsuarioService _usuarios;
    private readonly IImagenTicketService _imagenes;
    private readonly IDashboardService _dashboards;

    public DetailModel(ISolicitudService svc, IComentarioService com,
                       IAuditoriaService audit, INotificacionService notif,
                       IUsuarioService usuarios, IImagenTicketService imagenes,
                       IDashboardService dashboards)
    {
        _svc = svc;
        _com = com;
        _audit = audit;
        _notif = notif;
        _usuarios = usuarios;
        _imagenes = imagenes;
        _dashboards = dashboards;
    }

    public SolicitudDetalleDto? Solicitud { get; set; }
    public List<ComentarioDto> Comentarios { get; set; } = new();
    public List<ImagenTicketDto> Imagenes { get; set; } = new();

    // Indica si el usuario solicitante ya tiene permiso al dashboard
    public bool YaTienePermiso { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Solicitud = await _svc.ObtenerPorIdAsync(id);
        if (Solicitud == null) return NotFound();

        if (!UserHelper.EsAdmin(User) && Solicitud.SolicitadoPorID != UserHelper.GetUsuarioId(User))
            return Forbid();

        Comentarios = await _com.ObtenerPorTicketAsync("Solicitud", id);
        Imagenes = await _imagenes.ObtenerPorTicketAsync("Solicitud", id);

        // Verificar si ya tiene permiso (para mostrar u ocultar el botón)
        if (UserHelper.EsAdmin(User) && Solicitud.DashboardID.HasValue)
            YaTienePermiso = await _dashboards.VerificarAccesoAsync(
                Solicitud.DashboardID.Value, Solicitud.SolicitadoPorID);

        return Page();
    }

    // ── Acción rápida: otorgar permiso desde el detalle de la solicitud ──
    public async Task<IActionResult> OnPostOtorgarPermisoAsync(int solicitudId)
    {
        if (!UserHelper.EsAdmin(User)) return Forbid();

        var solicitud = await _svc.ObtenerPorIdAsync(solicitudId);
        if (solicitud == null) return NotFound();

        if (!solicitud.DashboardID.HasValue)
        {
            TempData["Error"] = "Esta solicitud no tiene un dashboard asociado.";
            return RedirectToPage(new { id = solicitudId });
        }

        var adminId = UserHelper.GetUsuarioId(User);
        var nombreAdmin = UserHelper.GetNombre(User);

        // Otorgar permiso
        await _dashboards.OtorgarPermisoAsync(
            solicitud.DashboardID.Value, solicitud.SolicitadoPorID, adminId);

        // Notificar al usuario
        await _notif.NotificarPermisoOtorgadoAsync(
            solicitud.SolicitadoPorID,
            solicitud.NombreDashboard ?? "Dashboard",
            solicitud.DashboardID.Value);

        // Auditoría
        await _audit.RegistrarAsync(adminId, "OtorgarPermisoDesdeSOL", "Dashboards",
            solicitud.DashboardID.Value,
            $"Permiso otorgado a UsuarioID:{solicitud.SolicitadoPorID} desde {solicitud.Folio}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Ok"] = $"Permiso otorgado a {solicitud.ReportadoPor} para \"{solicitud.NombreDashboard}\".";
        return RedirectToPage(new { id = solicitudId });
    }

    public async Task<IActionResult> OnPostAgregarComentarioAsync([FromBody] NuevoComentarioDto dto)
    {
        if (!ModelState.IsValid)
            return new JsonResult(new { success = false, error = "Datos inválidos." });

        var uid = UserHelper.GetUsuarioId(User);
        var esAdmin = UserHelper.EsAdmin(User);
        var nombreAutor = UserHelper.GetNombre(User);

        var nuevoId = await _com.AgregarAsync(dto.TipoRegistro, dto.RegistroId, uid, esAdmin, dto.Mensaje);
        var solicitud = await _svc.ObtenerPorIdAsync(dto.RegistroId);

        if (solicitud != null)
        {
            if (!esAdmin)
            {
                var admins = await _usuarios.ObtenerAdminsAsync();
                foreach (var admin in admins.Where(a => a.UsuarioID != uid))
                    await _notif.NotificarNuevoComentarioAsync(
                        admin.UsuarioID, solicitud.Folio, "Solicitud", dto.RegistroId, nombreAutor);
            }
            else
            {
                if (solicitud.SolicitadoPorID != uid)
                    await _notif.NotificarNuevoComentarioAsync(
                        solicitud.SolicitadoPorID, solicitud.Folio, "Solicitud", dto.RegistroId, nombreAutor);

                if (solicitud.AsignadoAID != null
                    && solicitud.AsignadoAID != uid
                    && solicitud.AsignadoAID != solicitud.SolicitadoPorID)
                    await _notif.NotificarNuevoComentarioAsync(
                        solicitud.AsignadoAID.Value, solicitud.Folio, "Solicitud", dto.RegistroId, nombreAutor);
            }
        }

        var comentario = new ComentarioDto
        {
            ComentarioID = nuevoId,
            Autor = nombreAutor,
            EsRespuestaAdmin = esAdmin,
            Mensaje = dto.Mensaje,
            FechaCreacion = DateTime.Now
        };
        return new JsonResult(new { success = true, comentario });
    }

    public async Task<IActionResult> OnPostCambiarEstatusAsync([FromBody] CambiarEstatusDto dto)
    {
        try
        {
            var solicitud = await _svc.ObtenerPorIdAsync(dto.RegistroId);
            if (solicitud == null)
                return new JsonResult(new { success = false, error = "Solicitud no encontrada." });

            await _svc.CambiarEstatusAsync(dto.RegistroId, dto.EstatusId, dto.AsignadoAId);

            var uid = UserHelper.GetUsuarioId(User);
            var nombreAdmin = UserHelper.GetNombre(User);

            await _audit.RegistrarAsync(uid, "CambiarEstatusSolicitud", "Solicitudes",
                dto.RegistroId, $"EstatusID:{dto.EstatusId}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            await _notif.NotificarCambioEstatusSolicitudAsync(
                solicitud.SolicitadoPorID, solicitud.Folio, dto.NuevoEstatusNombre, dto.RegistroId, nombreAdmin);

            if (dto.AsignadoAId.HasValue && dto.AsignadoAId != solicitud.SolicitadoPorID)
                await _notif.NotificarCambioEstatusSolicitudAsync(
                    dto.AsignadoAId.Value, solicitud.Folio, dto.NuevoEstatusNombre, dto.RegistroId, nombreAdmin);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}