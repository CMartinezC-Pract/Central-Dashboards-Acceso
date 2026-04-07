// ══════════════════════════════════════════════════════════════
//  Pages/Api/AvisosApi.cshtml.cs
//  Crear carpeta Pages/Api/ y colocar este archivo ahí
// ══════════════════════════════════════════════════════════════
using CentralDashboards.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Api;

[Authorize]
public class AvisosApiModel : PageModel
{
    private readonly IAvisoService _avisos;
    public AvisosApiModel(IAvisoService avisos) => _avisos = avisos;

    // GET /api/avisos/{id}/datos
    public async Task<IActionResult> OnGetDatosAsync(int id)
    {
        var uid = UserHelper.GetUsuarioId(User);
        var comentarios = await _avisos.ObtenerComentariosAsync(id);
        var reacciones = await _avisos.ObtenerReaccionesAsync(id, uid);

        return new JsonResult(new { comentarios, reacciones });
    }

    // POST /api/avisos/{id}/comentario
    public async Task<IActionResult> OnPostComentarioAsync(int id, [FromBody] ComentarioPayload body)
    {
        if (string.IsNullOrWhiteSpace(body?.Mensaje))
            return new JsonResult(new { error = "Mensaje vacío." }) { StatusCode = 400 };

        var uid = UserHelper.GetUsuarioId(User);
        await _avisos.AgregarComentarioAsync(id, uid, body.Mensaje.Trim());

        // Devolver comentarios actualizados
        var comentarios = await _avisos.ObtenerComentariosAsync(id);
        return new JsonResult(comentarios.LastOrDefault());
    }

    // POST /api/avisos/{id}/reaccion
    public async Task<IActionResult> OnPostReaccionAsync(int id, [FromBody] ReaccionPayload body)
    {
        if (string.IsNullOrWhiteSpace(body?.Tipo))
            return new JsonResult(new { error = "Tipo vacío." }) { StatusCode = 400 };

        var uid = UserHelper.GetUsuarioId(User);
        var reacciones = await _avisos.ToggleReaccionAsync(id, uid, body.Tipo);
        return new JsonResult(reacciones);
    }

    public class ComentarioPayload { public string? Mensaje { get; set; } }
    public class ReaccionPayload { public string? Tipo { get; set; } }
}