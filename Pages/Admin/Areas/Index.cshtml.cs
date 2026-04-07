using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;

namespace CentralDashboards.Pages.Admin.Areas;

[Authorize(Policy = "SoloAdmin")]
public class IndexModel : PageModel
{
    private readonly CentralDashboardsContext _db;

    public IndexModel(CentralDashboardsContext db)
    {
        _db = db;
    }

    public List<AreaDto> Areas { get; set; } = new();

    public async Task OnGetAsync()
        => Areas = await _db.Areas
            .Select(a => new AreaDto
            {
                AreaID = a.AreaID,
                NombreArea = a.NombreArea,
                Activo = a.Activo
            }).ToListAsync();

    public async Task<IActionResult> OnPostDesactivarAsync(int id)
    {
        var area = await _db.Areas.FindAsync(id);
        if (area != null) { area.Activo = false; await _db.SaveChangesAsync(); }
        TempData["Exito"] = "Área desactivada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivarAsync(int id)
    {
        var area = await _db.Areas.FindAsync(id);
        if (area != null) { area.Activo = true; await _db.SaveChangesAsync(); }
        TempData["Exito"] = "Área reactivada.";
        return RedirectToPage();
    }
}