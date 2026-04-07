using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Dashboards;

[Authorize(Policy = "SoloAdmin")]
public class CreateModel : PageModel
{
    private readonly IDashboardService _svc;
    private readonly IAuditoriaService _audit;
    private readonly IUnidadNegocioService _unidades;

    public CreateModel(IDashboardService svc, IAuditoriaService audit, IUnidadNegocioService unidades)
    {
        _svc = svc;
        _audit = audit;
        _unidades = unidades;
    }

    [BindProperty] public DashboardCreateDto Input { get; set; } = new();
    [BindProperty] public IFormFile? Imagen { get; set; }
    [BindProperty] public IFormFile? ArchivoDocumentacion { get; set; }

    public SelectList Unidades { get; set; } = default!;

    public async Task OnGetAsync() => await CargarUnidadesAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CargarUnidadesAsync(); return Page(); }

        // ── Imagen de previsualización ────────────────────────────
        if (Imagen is { Length: > 0 })
        {
            using var ms = new MemoryStream();
            await Imagen.CopyToAsync(ms);
            Input.ImagenData = ms.ToArray();
            Input.ImagenTipo = Imagen.ContentType;
            Input.ImagenURL = null;
        }

        // ── Documentación ─────────────────────────────────────────
        if (ArchivoDocumentacion is { Length: > 0 })
        {
            var tiposPermitidos = new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            };

            if (!tiposPermitidos.Contains(ArchivoDocumentacion.ContentType))
            {
                ModelState.AddModelError("ArchivoDocumentacion",
                    "Solo se permiten archivos PDF, Word o PowerPoint.");
                await CargarUnidadesAsync();
                return Page();
            }

            if (ArchivoDocumentacion.Length > 20 * 1024 * 1024)
            {
                ModelState.AddModelError("ArchivoDocumentacion",
                    "El archivo no puede superar 20 MB.");
                await CargarUnidadesAsync();
                return Page();
            }

            using var ms = new MemoryStream();
            await ArchivoDocumentacion.CopyToAsync(ms);
            Input.DocumentacionData = ms.ToArray();
            Input.DocumentacionNombre = ArchivoDocumentacion.FileName;
            Input.DocumentacionTipo = ArchivoDocumentacion.ContentType;
        }

        var uid = UserHelper.GetUsuarioId(User);
        var (id, folio) = await _svc.CrearAsync(Input, uid);

        await _audit.RegistrarAsync(uid, "CrearDashboard", "Dashboards", id,
            $"{folio} - {Input.Nombre}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = $"Dashboard {folio} registrado correctamente.";

        return Input.EsPrivado
            ? RedirectToPage("./Permisos", new { id })
            : RedirectToPage("./Index");
    }

    private async Task CargarUnidadesAsync()
    {
        var unidades = await _unidades.ObtenerTodasAsync();
        Unidades = new SelectList(unidades, "UnidadID", "Nombre");
    }
}