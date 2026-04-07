// ─────────────────────────────────────────────────────────────
//  Create.cshtml.cs  — Pages/Admin/UnidadesNegocio/
// ─────────────────────────────────────────────────────────────
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.UnidadesNegocio;

[Authorize(Policy = "SoloAdmin")]
public class CreateModel : PageModel
{
    private readonly IUnidadNegocioService _svc;
    private readonly IAuditoriaService _audit;

    public CreateModel(IUnidadNegocioService svc, IAuditoriaService audit)
    {
        _svc = svc;
        _audit = audit;
    }

    [BindProperty] public UnidadNegocioCreateDto Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var id = await _svc.CrearAsync(Input);
        var uid = UserHelper.GetUsuarioId(User);

        await _audit.RegistrarAsync(uid, "CrearUnidadNegocio", "UnidadesNegocio", id,
            Input.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = $"Unidad \"{Input.Nombre}\" creada correctamente.";
        return RedirectToPage("./Index");
    }
}