// ─────────────────────────────────────────────────────────────
//  Edit.cshtml.cs  — Pages/Admin/UnidadesNegocio/
// ─────────────────────────────────────────────────────────────
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.UnidadesNegocio;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly IUnidadNegocioService _svc;
    private readonly IAuditoriaService _audit;

    public EditModel(IUnidadNegocioService svc, IAuditoriaService audit)
    {
        _svc = svc;
        _audit = audit;
    }

    [BindProperty] public UnidadNegocioEditDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unidades = await _svc.ObtenerTodasAsync(soloActivas: false);
        var u = unidades.FirstOrDefault(x => x.UnidadID == id);
        if (u == null) return NotFound();

        Input = new UnidadNegocioEditDto
        {
            UnidadID = u.UnidadID,
            Nombre = u.Nombre,
            Descripcion = u.Descripcion,
            IconoURL = u.IconoURL,
            Activo = u.Activo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await _svc.ActualizarAsync(Input.UnidadID, new UnidadNegocioCreateDto
        {
            Nombre = Input.Nombre,
            Descripcion = Input.Descripcion,
            IconoURL = Input.IconoURL
        }, Input.Activo);

        var uid = UserHelper.GetUsuarioId(User);
        await _audit.RegistrarAsync(uid, "EditarUnidadNegocio", "UnidadesNegocio", Input.UnidadID,
            Input.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = $"Unidad \"{Input.Nombre}\" actualizada correctamente.";
        return RedirectToPage("./Index");
    }
}