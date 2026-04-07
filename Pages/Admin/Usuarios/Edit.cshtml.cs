using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.Usuarios;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly IUsuarioService _svc;
    private readonly IAuditoriaService _audit;
    private readonly CentralDashboardsContext _db;

    public EditModel(IUsuarioService svc, IAuditoriaService audit, CentralDashboardsContext db)
    {
        _svc = svc;
        _audit = audit;
        _db = db;
    }

    [BindProperty] public UsuarioEditDto Input { get; set; } = new();
    public string Correo { get; set; } = "";
    public SelectList Roles { get; set; } = default!;
    public SelectList Areas { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var u = await _svc.ObtenerPorIdAsync(id);
        if (u == null) return NotFound();
        Correo = u.Correo;
        Input = new UsuarioEditDto
        {
            UsuarioID = u.UsuarioID,
            Nombre = u.Nombre,
            RolID = u.RolID,
            AreaID = u.AreaID,
            Activo = u.Activo
        };
        await CargarListasAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CargarListasAsync(); return Page(); }
        var uid = UserHelper.GetUsuarioId(User);
        await _svc.ActualizarAsync(Input);
        await _audit.RegistrarAsync(uid, "EditarUsuario", "Usuarios", Input.UsuarioID,
            Input.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Exito"] = "Usuario actualizado correctamente.";
        return RedirectToPage("./Index");
    }

    private async Task CargarListasAsync()
    {
        Roles = new SelectList(await _svc.ObtenerRolesAsync(), "RolID", "NombreRol");
        var areas = await _db.Areas
            .Where(a => a.Activo)
            .Select(a => new { a.AreaID, a.NombreArea })
            .ToListAsync();
        Areas = new SelectList(areas, "AreaID", "NombreArea");
    }
}