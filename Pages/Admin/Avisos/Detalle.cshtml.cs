using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Avisos;

[Authorize]
public class DetalleModel : PageModel
{
    private readonly IAvisoService _avisos;

    public AvisoDto? Aviso { get; set; }
    public List<AvisoComentarioDto> Comentarios { get; set; } = new();

    public DetalleModel(IAvisoService avisos) => _avisos = avisos;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Aviso = await _avisos.ObtenerPorIdAsync(id);
        if (Aviso is null || !Aviso.Activo) return NotFound();

        Comentarios = await _avisos.ObtenerComentariosAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostComentarAsync(int avisoId, string mensaje)
    {
        if (string.IsNullOrWhiteSpace(mensaje))
        {
            TempData["ErrComentario"] = "El comentario no puede estar vacío.";
            return RedirectToPage(new { id = avisoId });
        }

        var usuarioId = UserHelper.GetUsuarioId(User);
        await _avisos.AgregarComentarioAsync(avisoId, usuarioId, mensaje.Trim());
        return RedirectToPage(new { id = avisoId });
    }
}