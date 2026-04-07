using CentralDashboards.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CentralDashboards.Pages.UnidadesNegocio;

public class IconoModel : PageModel
{
    private readonly CentralDashboardsContext _db;
    public IconoModel(CentralDashboardsContext db) => _db = db;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!Request.Query.TryGetValue("id", out var idStr) ||
            !int.TryParse(idStr, out int id))
            return NotFound();

        var unidad = await _db.UnidadesNegocio
            .Where(u => u.UnidadID == id)
            .Select(u => new { u.IconoURL })
            .FirstOrDefaultAsync();

        if (unidad?.IconoURL == null)
            return NotFound();

        var raw = unidad.IconoURL;
        if (raw.StartsWith("data:"))
        {
            var comma = raw.IndexOf(',');
            var mime = raw[5..raw.IndexOf(';')];
            var data = Convert.FromBase64String(raw[(comma + 1)..]);
            Response.Headers["Cache-Control"] = "public, max-age=86400";
            return File(data, mime);
        }

        return Redirect(raw);
    }
}