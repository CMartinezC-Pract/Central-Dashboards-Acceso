using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.Usuarios;

[Authorize(Policy = "SoloAdmin")]
public class IndexModel : PageModel
{
    private readonly IUsuarioService _svc;
    public IndexModel(IUsuarioService svc) => _svc = svc;
    public List<UsuarioDto> Usuarios { get; set; } = new();

    public async Task OnGetAsync() => Usuarios = await _svc.ObtenerTodosAsync(soloActivos: false);

    public async Task<IActionResult> OnGetDesactivarAsync(int id)
    {
        await _svc.DesactivarAsync(id);
        TempData["Exito"] = "Usuario desactivado.";
        return RedirectToPage();
    }
}
