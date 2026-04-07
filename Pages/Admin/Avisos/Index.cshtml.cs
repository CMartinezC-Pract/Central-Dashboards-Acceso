using CentralDashboards.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.Avisos;

[Authorize(Policy = "SoloAdmin")]
public class IndexModel : PageModel
{
    private readonly IAvisoService _avisos;
    public List<AvisoDto> Avisos { get; set; } = new();

    public IndexModel(IAvisoService avisos) => _avisos = avisos;

    public async Task OnGetAsync()
        => Avisos = await _avisos.ObtenerTodosAsync(soloActivos: false);

    public async Task<IActionResult> OnPostCambiarEstadoAsync(int id, bool activo)
    {
        await _avisos.CambiarEstadoAsync(id, activo);
        TempData["Ok"] = activo ? "Aviso activado." : "Aviso desactivado.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        await _avisos.EliminarAsync(id);
        TempData["Ok"] = "Aviso eliminado correctamente.";
        return RedirectToPage();
    }
}