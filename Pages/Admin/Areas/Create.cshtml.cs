using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.Areas;

[Authorize(Policy = "SoloAdmin")]
public class CreateModel : PageModel
{
    private readonly CentralDashboardsContext _db;
    private readonly IAuditoriaService _audit;
    public CreateModel(CentralDashboardsContext db, IAuditoriaService audit) { _db = db; _audit = audit; }

    [BindProperty] public AreaCreateDto Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Areas_Crear @NombreArea, @Descripcion, @IconoURL, @NuevoID OUTPUT",
            new SqlParameter("@NombreArea", Input.NombreArea),
            new SqlParameter("@Descripcion", (object)DBNull.Value),
            new SqlParameter("@IconoURL", (object)DBNull.Value),
            pId);

        var uid = UserHelper.GetUsuarioId(User);
        await _audit.RegistrarAsync(uid, "CrearArea", "Areas", (int)pId.Value, Input.NombreArea,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = $"Área '{Input.NombreArea}' creada correctamente.";
        return RedirectToPage("./Index");
    }
}