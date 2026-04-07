using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static CentralDashboards.Services.NotificacionService;

namespace CentralDashboards.Pages.Admin.Avisos;

[Authorize(Policy = "SoloAdmin")]
public class CreateModel : PageModel
{
    private readonly IAvisoService _avisos;
    private readonly INotificacionService _notif;

    [BindProperty]
    public AvisoCreateDto Input { get; set; } = new();

    public CreateModel(IAvisoService avisos, INotificacionService notif)
    {
        _avisos = avisos;
        _notif = notif;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var usuarioId = UserHelper.GetUsuarioId(User);
        var nombreAdmin = User.FindFirst("Nombre")?.Value ?? "Un administrador";

        var avisoId = await _avisos.CrearAsync(Input, usuarioId);
        await _notif.NotificarNuevoAvisoAsync(Input.Titulo, avisoId, nombreAdmin);

        TempData["Ok"] = "Aviso publicado correctamente.";
        return RedirectToPage("Index");
    }
}