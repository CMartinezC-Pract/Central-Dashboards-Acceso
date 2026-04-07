using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using CentralDashboards.Data;
using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;
using Microsoft.EntityFrameworkCore;

namespace CentralDashboards.Pages.Admin.Areas;

[Authorize(Policy = "SoloAdmin")]
public class EditModel : PageModel
{
    private readonly CentralDashboardsContext _db;
    private readonly IAuditoriaService _audit;
    public EditModel(CentralDashboardsContext db, IAuditoriaService audit) { _db = db; _audit = audit; }

    [BindProperty] public AreaEditDto Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var area = await _db.Areas.FindAsync(id);
        if (area == null) return NotFound();

        Input = new AreaEditDto
        {
            AreaID = area.AreaID,
            NombreArea = area.NombreArea,
            Activo = area.Activo
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Areas_Actualizar @AreaID, @NombreArea, @Descripcion, @IconoURL, @Activo",
            new SqlParameter("@AreaID", Input.AreaID),
            new SqlParameter("@NombreArea", Input.NombreArea),
            new SqlParameter("@Descripcion", (object)DBNull.Value),
            new SqlParameter("@IconoURL", (object)DBNull.Value),
            new SqlParameter("@Activo", Input.Activo));

        var uid = UserHelper.GetUsuarioId(User);
        await _audit.RegistrarAsync(uid, "EditarArea", "Areas", Input.AreaID, Input.NombreArea,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Exito"] = $"Área '{Input.NombreArea}' actualizada.";
        return RedirectToPage("./Index");
    }
}

public class AreaEditDto : AreaCreateDto
{
    public int AreaID { get; set; }
    public bool Activo { get; set; } = true;
}