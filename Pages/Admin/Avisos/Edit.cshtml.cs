using CentralDashboards.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Services;
namespace CentralDashboards.Pages.Admin.Avisos;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly IAvisoService _avisos;

    [BindProperty]
    public AvisoEditDto Input { get; set; } = new();

    public EditModel(IAvisoService avisos) => _avisos = avisos;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var aviso = await _avisos.ObtenerPorIdAsync(id);
        if (aviso is null) return NotFound();

        Input = new AvisoEditDto
        {
            Id = aviso.Id,
            Titulo = aviso.Titulo,
            Contenido = aviso.Contenido,
            Tipo = aviso.Tipo,
            Fecha = aviso.Fecha,
            Activo = aviso.Activo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await _avisos.ActualizarAsync(Input);
        TempData["Ok"] = "Aviso actualizado correctamente.";
        return RedirectToPage("Index");
    }
}