using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;
using CentralDashboards.Models.Dtos;

namespace CentralDashboards.Pages.Admin.Catalogos;

[Authorize(Policy = "SoloAdmin")]
public class IndexModel : PageModel
{
    private readonly CentralDashboardsContext _db;
    public IndexModel(CentralDashboardsContext db) => _db = db;

    public List<EstatusDto> Estatus          { get; set; } = new();
    public List<TipoDto>    TiposIncidencia  { get; set; } = new();
    public List<TipoDto>    TiposSolicitud   { get; set; } = new();

    public async Task OnGetAsync()
    {
        Estatus = await _db.Estatus
            .Select(e => new EstatusDto { EstatusID = e.EstatusID, NombreEstatus = e.NombreEstatus, AplicaA = e.AplicaA, Color = e.Color })
            .OrderBy(e => e.AplicaA).ThenBy(e => e.NombreEstatus).ToListAsync();

        TiposIncidencia = await _db.TiposIncidencia
            .Select(t => new TipoDto { TipoID = t.TipoID, Nombre = t.Nombre, Descripcion = t.Descripcion })
            .OrderBy(t => t.Nombre).ToListAsync();

        TiposSolicitud = await _db.TiposSolicitud
            .Select(t => new TipoDto { TipoID = t.TipoID, Nombre = t.Nombre, Descripcion = t.Descripcion })
            .OrderBy(t => t.Nombre).ToListAsync();
    }

    // ══════════════════════════════════════════════════════
    //  ESTATUS
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> OnPostCrearEstatusAsync([FromBody] EstatusRequest req)
    {
        try
        {
            var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_Estatus_Crear @NombreEstatus, @AplicaA, @Color, @NuevoID OUTPUT",
                new SqlParameter("@NombreEstatus", req.Nombre),
                new SqlParameter("@AplicaA",       req.AplicaA),
                new SqlParameter("@Color",         req.Color ?? (object)DBNull.Value),
                pId);
            return new JsonResult(new { success = true, id = (int)pId.Value });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostActualizarEstatusAsync([FromBody] EstatusRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_Estatus_Actualizar @EstatusID, @NombreEstatus, @AplicaA, @Color",
                new SqlParameter("@EstatusID",     req.Id),
                new SqlParameter("@NombreEstatus", req.Nombre),
                new SqlParameter("@AplicaA",       req.AplicaA),
                new SqlParameter("@Color",         req.Color ?? (object)DBNull.Value));
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostEliminarEstatusAsync([FromBody] IdRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_Estatus_Eliminar @EstatusID",
                new SqlParameter("@EstatusID", req.Id));
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    // ══════════════════════════════════════════════════════
    //  TIPOS INCIDENCIA
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> OnPostCrearIncidenciaTipoAsync([FromBody] TipoRequest req)
    {
        try
        {
            var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_TiposIncidencia_Crear @Nombre, @Descripcion, @NuevoID OUTPUT",
                new SqlParameter("@Nombre",      req.Nombre),
                new SqlParameter("@Descripcion", req.Descripcion ?? (object)DBNull.Value),
                pId);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostActualizarIncidenciaTipoAsync([FromBody] TipoRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_TiposIncidencia_Actualizar @TipoID, @Nombre, @Descripcion",
                new SqlParameter("@TipoID",      req.Id),
                new SqlParameter("@Nombre",      req.Nombre),
                new SqlParameter("@Descripcion", req.Descripcion ?? (object)DBNull.Value));
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostEliminarIncidenciaTipoAsync([FromBody] IdRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_TiposIncidencia_Eliminar @TipoID",
                new SqlParameter("@TipoID", req.Id));
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    // ══════════════════════════════════════════════════════
    //  TIPOS SOLICITUD
    // ══════════════════════════════════════════════════════
    public async Task<IActionResult> OnPostCrearSolicitudTipoAsync([FromBody] TipoRequest req)
    {
        try
        {
            var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_TiposSolicitud_Crear @Nombre, @Descripcion, @NuevoID OUTPUT",
                new SqlParameter("@Nombre",      req.Nombre),
                new SqlParameter("@Descripcion", req.Descripcion ?? (object)DBNull.Value),
                pId);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostActualizarSolicitudTipoAsync([FromBody] TipoRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_TiposSolicitud_Actualizar @TipoID, @Nombre, @Descripcion",
                new SqlParameter("@TipoID",      req.Id),
                new SqlParameter("@Nombre",      req.Nombre),
                new SqlParameter("@Descripcion", req.Descripcion ?? (object)DBNull.Value));
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }

    public async Task<IActionResult> OnPostEliminarSolicitudTipoAsync([FromBody] IdRequest req)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_TiposSolicitud_Eliminar @TipoID",
                new SqlParameter("@TipoID", req.Id));
            return new JsonResult(new { success = true });
        }
        catch (Exception ex) { return new JsonResult(new { success = false, error = ex.Message }); }
    }
}

// ── Request DTOs para los handlers AJAX ──────────────────
public class EstatusRequest
{
    public int     Id      { get; set; }
    public string  Nombre  { get; set; } = "";
    public string  AplicaA { get; set; } = "Ambos";
    public string? Color   { get; set; }
}

public class TipoRequest
{
    public int     Id          { get; set; }
    public string  Nombre      { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class IdRequest
{
    public int Id { get; set; }
}
