using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CentralDashboards.Helpers;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Notificaciones;

[Authorize]
[IgnoreAntiforgeryToken]
public class ApiModel : PageModel
{
    private readonly INotificacionService _notif;
    public ApiModel(INotificacionService notif) => _notif = notif;

    // GET ?handler=Listar
    public async Task<IActionResult> OnGetListarAsync()
    {
        var uid = UserHelper.GetUsuarioId(User);
        var list = await _notif.ObtenerPorUsuarioAsync(uid, 20);
        return new JsonResult(list);
    }

    // GET ?handler=Contar
    public async Task<IActionResult> OnGetContarAsync()
    {
        var uid = UserHelper.GetUsuarioId(User);
        var count = await _notif.ContarNoLeidasAsync(uid);
        return new JsonResult(new { count });
    }

    // POST ?handler=MarcarLeida
    public async Task<IActionResult> OnPostMarcarLeidaAsync([FromBody] MarcarLeidaRequest req)
    {
        await _notif.MarcarLeidaAsync(req.Id);
        return new JsonResult(new { success = true });
    }

    // POST ?handler=MarcarTodas
    public async Task<IActionResult> OnPostMarcarTodasAsync()
    {
        var uid = UserHelper.GetUsuarioId(User);
        await _notif.MarcarTodasLeidasAsync(uid);
        return new JsonResult(new { success = true });
    }
}

public class MarcarLeidaRequest { public int Id { get; set; } }