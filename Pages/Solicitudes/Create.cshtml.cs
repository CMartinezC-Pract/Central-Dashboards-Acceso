using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Solicitudes;

[Authorize]
[IgnoreAntiforgeryToken]
public class CreateModel : PageModel
{
    private readonly ISolicitudService _svc;
    private readonly IDashboardService _dash;
    private readonly IAuditoriaService _audit;
    private readonly INotificacionService _notif;
    private readonly IUsuarioService _usuarios;
    private readonly IImagenTicketService _imagenes;

    public CreateModel(ISolicitudService svc, IDashboardService dash,
                       IAuditoriaService audit, INotificacionService notif,
                       IUsuarioService usuarios, IImagenTicketService imagenes)
    {
        _svc = svc;
        _dash = dash;
        _audit = audit;
        _notif = notif;
        _usuarios = usuarios;
        _imagenes = imagenes;
    }

    [BindProperty] public SolicitudCreateDto Input { get; set; } = new();

    // Archivos adjuntos (multi-imagen)
    [BindProperty] public List<IFormFile>? Imagenes { get; set; }

    public SelectList Dashboards { get; set; } = default!;
    public SelectList Tipos { get; set; } = default!;

    [BindProperty(SupportsGet = true)] public int? DashboardId { get; set; }

    public async Task OnGetAsync()
    {
        var todos = await _dash.ObtenerTodosAsync();
        Dashboards = new SelectList(todos, "DashboardID", "Nombre");
        await CargarTiposAsync();
        if (DashboardId.HasValue) Input.DashboardID = DashboardId.Value;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var t = await _dash.ObtenerTodosAsync();
            Dashboards = new SelectList(t, "DashboardID", "Nombre");
            await CargarTiposAsync();
            return Page();
        }

        var uid = UserHelper.GetUsuarioId(User);
        var nombreUsuario = UserHelper.GetNombre(User);

        var (id, folio) = await _svc.CrearAsync(Input, uid);

        // Guardar imágenes si vienen adjuntas
        if (Imagenes != null && Imagenes.Count > 0)
            await _imagenes.GuardarImagenesAsync("Solicitud", id, uid, Imagenes);

        await _audit.RegistrarAsync(uid, "CrearSolicitud", "Solicitudes", id, folio,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var admins = await _usuarios.ObtenerAdminsAsync();
        foreach (var admin in admins)
            await _notif.NotificarNuevaSolicitudAsync(admin.UsuarioID, folio, Input.Titulo, id, nombreUsuario);

        TempData["Exito"] = $"Solicitud {folio} registrada correctamente.";
        return RedirectToPage("./Detail", new { id });
    }

    // ── Handler AJAX (desde modal en Ver.cshtml) ───────────────────────────
    public async Task<IActionResult> OnPostAjaxAsync([FromBody] SolicitudCreateDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Titulo) ||
            string.IsNullOrWhiteSpace(dto.Descripcion) || dto.TipoSolicitudID == 0)
            return new JsonResult(new { message = "Datos incompletos." }) { StatusCode = 400 };

        var uid = UserHelper.GetUsuarioId(User);
        var nombreUsuario = UserHelper.GetNombre(User);

        var (id, folio) = await _svc.CrearAsync(dto, uid);

        await _audit.RegistrarAsync(uid, "CrearSolicitud", "Solicitudes", id, folio,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        var admins = await _usuarios.ObtenerAdminsAsync();
        foreach (var admin in admins)
            await _notif.NotificarNuevaSolicitudAsync(admin.UsuarioID, folio, dto.Titulo, id, nombreUsuario);

        return new JsonResult(new { id, folio });
    }

    private async Task CargarTiposAsync()
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var db = scope.ServiceProvider
                      .GetRequiredService<CentralDashboards.Data.CentralDashboardsContext>();
        var tipos = db.TiposSolicitud.Select(t => new { t.TipoID, t.Nombre }).ToList();
        Tipos = new SelectList(tipos, "TipoID", "Nombre");
    }

    // ─────────────────────────────────────────────────────────────
    // AGREGAR en Pages/Solicitudes/Create.cshtml.cs
    // ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> OnPostAjaxFormAsync(
        [FromForm] int? DashboardID,
        [FromForm] int TipoSolicitudID,
        [FromForm] string Titulo,
        [FromForm] string Descripcion,
        [FromForm] string Prioridad,
        [FromForm] List<IFormFile>? Imagenes)
    {
        try
        {
            var uid = UserHelper.GetUsuarioId(User);
            var dto = new SolicitudCreateDto
            {
                DashboardID = DashboardID == 0 ? null : DashboardID,
                TipoSolicitudID = TipoSolicitudID,
                Titulo = Titulo,
                Descripcion = Descripcion,
                Prioridad = Prioridad
            };
            var (id, folio) = await _svc.CrearAsync(dto, uid);

            // Guardar imágenes si las hay
            if (Imagenes != null && Imagenes.Count > 0)
                await _imagenes.GuardarImagenesAsync("Solicitud", id, uid, Imagenes);

            // Notificar a admins
            var nombre = UserHelper.GetNombre(User);
            var admins = await _usuarios.ObtenerAdminsAsync();
            foreach (var admin in admins.Where(a => a.UsuarioID != uid))
                await _notif.NotificarNuevaSolicitudAsync(admin.UsuarioID, folio, Titulo, id, nombre);

            await _audit.RegistrarAsync(uid, "CrearSolicitud", "Solicitudes", id,
                $"Folio:{folio}", HttpContext.Connection.RemoteIpAddress?.ToString());

            return new JsonResult(new { folio, id });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { message = ex.Message }) { StatusCode = 500 };
        }
    }
}
