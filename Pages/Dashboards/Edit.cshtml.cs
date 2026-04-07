using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Dashboards;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly IDashboardService _svc;
    private readonly IAuditoriaService _audit;
    private readonly IUnidadNegocioService _unidades;

    public EditModel(IDashboardService svc, IAuditoriaService audit, IUnidadNegocioService unidades)
    {
        _svc = svc;
        _audit = audit;
        _unidades = unidades;
    }

    [BindProperty] public DashboardEditDto Input { get; set; } = new();
    [BindProperty] public IFormFile? Imagen { get; set; }
    [BindProperty] public IFormFile? ArchivoDocumentacion { get; set; }

    public string Folio { get; set; } = "";
    public SelectList Unidades { get; set; } = default!;

    // Imagen actual
    public string? ImagenActual { get; set; }

    // Documentación actual
    public bool TieneDocumentacion { get; set; }
    public string? DocumentacionNombreActual { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var d = await _svc.ObtenerPorIdAsync(id);
        if (d == null) return NotFound();

        Folio = d.Folio;
        ImagenActual = d.ImagenPrincipal;
        TieneDocumentacion = d.TieneDocumentacion;
        DocumentacionNombreActual = d.DocumentacionNombre;

        Input = new DashboardEditDto
        {
            DashboardID = d.DashboardID,
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            UnidadID = d.UnidadID,
            URLReporte = d.URLReporte,
            EsPrivado = d.EsPrivado,
            Activo = d.Activo
        };
        await CargarUnidadesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CargarUnidadesAsync(); return Page(); }

        // ── Imagen ────────────────────────────────────────────────
        if (Imagen is { Length: > 0 })
        {
            var tiposImg = new[] { "image/png", "image/jpeg", "image/webp", "image/gif" };
            if (!tiposImg.Contains(Imagen.ContentType))
            {
                ModelState.AddModelError("Imagen", "Solo se permiten imágenes PNG, JPG, WEBP o GIF.");
                await CargarUnidadesAsync();
                return Page();
            }
            if (Imagen.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("Imagen", "La imagen no puede superar 5 MB.");
                await CargarUnidadesAsync();
                return Page();
            }
            using var ms = new MemoryStream();
            await Imagen.CopyToAsync(ms);
            Input.ImagenData = ms.ToArray();
            Input.ImagenTipo = Imagen.ContentType;
            Input.ImagenURL = null;
        }

        // ── Documentación ─────────────────────────────────────────
        if (ArchivoDocumentacion is { Length: > 0 })
        {
            var tiposDoc = new[]
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation"
            };

            if (!tiposDoc.Contains(ArchivoDocumentacion.ContentType))
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
        await _svc.ActualizarAsync(Input);
        await _audit.RegistrarAsync(uid, "EditarDashboard", "Dashboards",
            Input.DashboardID, Input.Nombre,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = "Dashboard actualizado correctamente.";
        return RedirectToPage("./Index");
    }

    private async Task CargarUnidadesAsync()
    {
        var unidades = await _unidades.ObtenerTodasAsync();
        Unidades = new SelectList(unidades, "UnidadID", "Nombre");
    }
}