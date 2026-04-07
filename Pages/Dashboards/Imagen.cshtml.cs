// Pages/Dashboards/Imagen.cshtml.cs
// Este archivo sirve la imagen binaria de un dashboard como respuesta HTTP
// El <img> en las vistas apunta a: /Dashboards/Imagen?id=1

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;

namespace CentralDashboards.Pages.Dashboards;

public class ImagenModel : PageModel
{
    private readonly CentralDashboardsContext _db;
    public ImagenModel(CentralDashboardsContext db) => _db = db;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var dash = await _db.Dashboards
            .Where(d => d.DashboardID == id && d.ImagenData != null)
            .Select(d => new { d.ImagenData, d.ImagenTipo })
            .FirstOrDefaultAsync();

        if (dash?.ImagenData == null)
            return NotFound();

        var tipo = dash.ImagenTipo ?? "image/png";
        return File(dash.ImagenData, tipo);
    }
}