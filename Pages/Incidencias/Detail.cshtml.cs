using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Incidencias;

[Authorize]
public class DetailModel : PageModel
{
    private readonly IIncidenciaService _svc;
    private readonly IComentarioService _com;
    private readonly IAuditoriaService _audit;
    private readonly INotificacionService _notif;
    private readonly IUsuarioService _usuarios;
    private readonly IImagenTicketService _imagenes;

    public DetailModel(IIncidenciaService svc, IComentarioService com,
                       IAuditoriaService audit, INotificacionService notif,
                       IUsuarioService usuarios, IImagenTicketService imagenes)
    {
        _svc = svc;
        _com = com;
        _audit = audit;
        _notif = notif;
        _usuarios = usuarios;
        _imagenes = imagenes;
    }

    public IncidenciaDetalleDto? Incidencia { get; set; }
    public List<ComentarioDto> Comentarios { get; set; } = new();
    public List<ImagenTicketDto> Imagenes { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Incidencia = await _svc.ObtenerPorIdAsync(id);
        if (Incidencia == null) return NotFound();

        if (!UserHelper.EsAdmin(User) && Incidencia.ReportadoPorID != UserHelper.GetUsuarioId(User))
            return Forbid();

        Comentarios = await _com.ObtenerPorTicketAsync("Incidencia", id);
        Imagenes = await _imagenes.ObtenerPorTicketAsync("Incidencia", id);
        return Page();
    }

    public async Task<IActionResult> OnPostAgregarComentarioAsync([FromBody] NuevoComentarioDto dto)
    {
        if (!ModelState.IsValid)
            return new JsonResult(new { success = false, error = "Datos inválidos." });

        var uid = UserHelper.GetUsuarioId(User);
        var esAdmin = UserHelper.EsAdmin(User);
        var nombreAutor = UserHelper.GetNombre(User);

        var nuevoId = await _com.AgregarAsync(dto.TipoRegistro, dto.RegistroId, uid, esAdmin, dto.Mensaje);
        var incidencia = await _svc.ObtenerPorIdAsync(dto.RegistroId);

        if (incidencia != null)
        {
            if (!esAdmin)
            {
                var admins = await _usuarios.ObtenerAdminsAsync();
                foreach (var admin in admins.Where(a => a.UsuarioID != uid))
                    await _notif.NotificarNuevoComentarioAsync(
                        admin.UsuarioID, incidencia.Folio, "Incidencia", dto.RegistroId, nombreAutor);
            }
            else
            {
                if (incidencia.ReportadoPorID != uid)
                    await _notif.NotificarNuevoComentarioAsync(
                        incidencia.ReportadoPorID, incidencia.Folio, "Incidencia", dto.RegistroId, nombreAutor);

                if (incidencia.AsignadoAID != null
                    && incidencia.AsignadoAID != uid
                    && incidencia.AsignadoAID != incidencia.ReportadoPorID)
                    await _notif.NotificarNuevoComentarioAsync(
                        incidencia.AsignadoAID.Value, incidencia.Folio, "Incidencia", dto.RegistroId, nombreAutor);
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
            var incidencia = await _svc.ObtenerPorIdAsync(dto.RegistroId);
            if (incidencia == null)
                return new JsonResult(new { success = false, error = "Incidencia no encontrada." });

            await _svc.CambiarEstatusAsync(dto.RegistroId, dto.EstatusId, dto.AsignadoAId);

            var uid = UserHelper.GetUsuarioId(User);
            var nombreAdmin = UserHelper.GetNombre(User);

            await _audit.RegistrarAsync(uid, "CambiarEstatusIncidencia", "Incidencias",
                dto.RegistroId, $"EstatusID:{dto.EstatusId}",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            await _notif.NotificarCambioEstatusIncidenciaAsync(
                incidencia.ReportadoPorID, incidencia.Folio, dto.NuevoEstatusNombre, dto.RegistroId, nombreAdmin);

            if (dto.AsignadoAId.HasValue && dto.AsignadoAId != incidencia.ReportadoPorID)
                await _notif.NotificarCambioEstatusIncidenciaAsync(
                    dto.AsignadoAId.Value, incidencia.Folio, dto.NuevoEstatusNombre, dto.RegistroId, nombreAdmin);

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }
}

public class CambiarEstatusDto
{
    public int RegistroId { get; set; }
    public int EstatusId { get; set; }
    public int? AsignadoAId { get; set; }
    public string NuevoEstatusNombre { get; set; } = "";
}
