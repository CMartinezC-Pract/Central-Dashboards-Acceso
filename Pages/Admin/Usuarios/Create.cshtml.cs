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
public class CreateModel : PageModel
{
    private readonly IUsuarioService _svc;
    private readonly IAuditoriaService _audit;
    private readonly CentralDashboardsContext _db;

    public CreateModel(IUsuarioService svc, IAuditoriaService audit, CentralDashboardsContext db)
    {
        _svc = svc;
        _audit = audit;
        _db = db;
    }

    [BindProperty] public UsuarioCreateDto Input { get; set; } = new();
    public SelectList Roles { get; set; } = default!;
    public SelectList Areas { get; set; } = default!;

    public async Task OnGetAsync() => await CargarListasAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await CargarListasAsync(); return Page(); }

        try
        {
            var id = await _svc.CrearAsync(Input);
            var uid = UserHelper.GetUsuarioId(User);
            await _audit.RegistrarAsync(uid, "CrearUsuario", "Usuarios", id, Input.Nombre,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            TempData["Exito"] = $"Usuario {Input.Nombre} creado correctamente.";
            return RedirectToPage("./Index");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("ya existe", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Input.Correo", "Ya existe un usuario registrado con ese correo.");
            await CargarListasAsync();
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Ocurrió un error al crear el usuario: {ex.Message}");
            await CargarListasAsync();
            return Page();
        }
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