using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CentralDashboards.Data;
using CentralDashboards.Models.Dtos;

namespace CentralDashboards.Services;

// ══════════════════════════════════════════════════════════════
//  INTERFACES
// ══════════════════════════════════════════════════════════════
public interface IDashboardService
{
    Task<List<UnidadNegocioDto>> ObtenerUnidadesAsync();
    Task<List<DashboardDto>> ObtenerPorUnidadAsync(int unidadId, int usuarioId);
    Task<List<DashboardDto>> ObtenerTodosAsync(int? unidadId = null, bool soloActivos = true, int? usuarioId = null);
    Task<DashboardDto?> ObtenerPorIdAsync(int dashboardId);
    Task<(int id, string folio)> CrearAsync(DashboardCreateDto dto, int creadoPorId);
    Task ActualizarAsync(DashboardEditDto dto);
    Task DesactivarAsync(int dashboardId);
    Task<List<DashboardPermisoDto>> ObtenerPermisosAsync(int dashboardId);
    Task OtorgarPermisoAsync(int dashboardId, int usuarioId, int otorgadoPorId);
    Task RevocarPermisoAsync(int dashboardId, int usuarioId);
    Task<bool> VerificarAccesoAsync(int dashboardId, int usuarioId);
}

public interface IUnidadNegocioService
{
    Task<List<UnidadNegocioDto>> ObtenerTodasAsync(bool soloActivas = true);
    Task<int> CrearAsync(UnidadNegocioCreateDto dto);
    Task ActualizarAsync(int id, UnidadNegocioCreateDto dto, bool activo);
}

public interface IIncidenciaService
{
    Task<List<IncidenciaResumenDto>> ObtenerTodasAsync(int? dashboardId = null, int? usuarioId = null, int? estatusId = null);
    Task<IncidenciaDetalleDto?> ObtenerPorIdAsync(int incidenciaId);
    Task<(int id, string folio)> CrearAsync(IncidenciaCreateDto dto, int reportadoPorId);
    Task ActualizarAsync(IncidenciaEditDto dto);
    Task CambiarEstatusAsync(int incidenciaId, int estatusId, int? asignadoAId = null);
    Task EliminarAsync(int incidenciaId);
}

public interface ISolicitudService
{
    Task<List<SolicitudResumenDto>> ObtenerTodasAsync(int? dashboardId = null, int? usuarioId = null, int? estatusId = null);
    Task<SolicitudDetalleDto?> ObtenerPorIdAsync(int solicitudId);
    Task<(int id, string folio)> CrearAsync(SolicitudCreateDto dto, int solicitadoPorId);
    Task ActualizarAsync(SolicitudEditDto dto);
    Task CambiarEstatusAsync(int solicitudId, int estatusId, int? asignadoAId = null);
    Task EliminarAsync(int solicitudId);
    Task<HashSet<int>> ObtenerDashboardsYaSolicitadosAsync(int usuarioId);
}

public interface IComentarioService
{
    Task<List<ComentarioDto>> ObtenerPorTicketAsync(string tipoRegistro, int registroId);
    Task<int> AgregarAsync(string tipoRegistro, int registroId, int autorId, bool esAdmin, string mensaje);
    Task EliminarAsync(int comentarioId);
}

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ObtenerTodosAsync(bool soloActivos = true);
    Task<UsuarioDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo);
    // ObtenerHashAsync eliminado — columna ContrasenaHash ya no existe en BD
    Task<int> CrearAsync(UsuarioCreateDto dto);
    Task ActualizarAsync(UsuarioEditDto dto);
    Task DesactivarAsync(int id);
    Task ActualizarUltimoAccesoAsync(int id);
    Task<List<RolDto>> ObtenerRolesAsync();
    Task<List<UsuarioDto>> ObtenerAdminsAsync();
}

public interface IAuditoriaService
{
    Task RegistrarAsync(int usuarioId, string accion, string? tabla = null, int? registroId = null, string? detalle = null, string? ip = null);
    Task<List<AuditoriaDto>> ObtenerRecentesAsync(int top = 100);
    Task<List<AuditoriaDto>> ObtenerPorUsuarioAsync(int usuarioId, int top = 50);
}

public interface INotificacionService
{
    Task CrearAsync(int usuarioId, string titulo, string mensaje, string tipo,
                    string? iconoClass = null, string? colorClass = null, string? url = null);
    Task<List<NotificacionDto>> ObtenerPorUsuarioAsync(int usuarioId, int top = 20);
    Task<int> ContarNoLeidasAsync(int usuarioId);
    Task MarcarLeidaAsync(int notificacionId);
    Task MarcarTodasLeidasAsync(int usuarioId);
    Task NotificarNuevaIncidenciaAsync(int adminId, string folio, string titulo, int incidenciaId, string nombreUsuario);
    Task NotificarNuevaSolicitudAsync(int adminId, string folio, string titulo, int solicitudId, string nombreUsuario);
    Task NotificarCambioEstatusIncidenciaAsync(int usuarioId, string folio, string nuevoEstatus, int incidenciaId, string nombreAdmin = "Un administrador");
    Task NotificarCambioEstatusSolicitudAsync(int usuarioId, string folio, string nuevoEstatus, int solicitudId, string nombreAdmin = "Un administrador");
    Task NotificarNuevoComentarioAsync(int usuarioId, string folio, string tipoTicket, int registroId, string nombreAutor);
    Task NotificarPermisoOtorgadoAsync(int usuarioId, string nombreDashboard, int dashboardId);
    Task NotificarPermisoRevocadoAsync(int usuarioId, string nombreDashboard);
    Task NotificarNuevoAvisoAsync(string titulo, int avisoId, string? nombreAdmin);
}

// ── IAvisoService — movida FUERA de NotificacionService (fix de estructura) ──
public interface IAvisoService
{
    Task<List<AvisoDto>> ObtenerTodosAsync(bool soloActivos = true);
    Task<AvisoDto?> ObtenerPorIdAsync(int id);
    Task<int> CrearAsync(AvisoCreateDto dto, int creadoPorId);
    Task ActualizarAsync(AvisoEditDto dto);
    Task CambiarEstadoAsync(int id, bool activo);
    Task EliminarAsync(int id);
    Task<List<AvisoComentarioDto>> ObtenerComentariosAsync(int avisoId);
    Task<int> AgregarComentarioAsync(int avisoId, int usuarioId, string mensaje);
    Task EliminarComentarioAsync(int comentarioId);
    Task<List<AvisoReaccionDto>> ObtenerReaccionesAsync(int avisoId, int usuarioId);
    Task<List<AvisoReaccionDto>> ToggleReaccionAsync(int avisoId, int usuarioId, string tipo);
}

// ══════════════════════════════════════════════════════════════
//  DASHBOARD SERVICE
// ══════════════════════════════════════════════════════════════
public class DashboardService : IDashboardService
{
    private readonly CentralDashboardsContext _db;
    public DashboardService(CentralDashboardsContext db) => _db = db;

    // CORRECCIÓN: método duplicado eliminado — solo existe la versión con usuarioId
    public async Task<List<DashboardDto>> ObtenerTodosAsync(int? unidadId = null, bool soloActivos = true, int? usuarioId = null)
        => await _db.Set<DashboardDto>()
            .FromSqlRaw("EXEC sp_Dashboards_ObtenerTodos @UnidadID, @SoloActivos, @UsuarioID",
                new SqlParameter("@UnidadID", unidadId ?? (object)DBNull.Value),
                new SqlParameter("@SoloActivos", soloActivos),
                new SqlParameter("@UsuarioID", usuarioId ?? (object)DBNull.Value))
            .ToListAsync();

    public async Task<List<DashboardDto>> ObtenerPorUnidadAsync(int unidadId, int usuarioId)
        => await _db.Set<DashboardDto>()
            .FromSqlRaw("EXEC sp_Dashboards_ObtenerTodos @UnidadID, @SoloActivos, @UsuarioID",
                new SqlParameter("@UnidadID", unidadId),
                new SqlParameter("@SoloActivos", true),
                new SqlParameter("@UsuarioID", usuarioId))
            .ToListAsync();

    public async Task<DashboardDto?> ObtenerPorIdAsync(int dashboardId)
        => (await _db.Set<DashboardDto>()
            .FromSqlRaw("EXEC sp_Dashboards_ObtenerPorID @DashboardID",
                new SqlParameter("@DashboardID", dashboardId))
            .ToListAsync()).FirstOrDefault();

    public async Task<(int id, string folio)> CrearAsync(DashboardCreateDto dto, int creadoPorId)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var pFolio = new SqlParameter("@Folio", System.Data.SqlDbType.NVarChar, 20) { Direction = System.Data.ParameterDirection.Output };

        var pImagenData = new SqlParameter("@ImagenData", System.Data.SqlDbType.VarBinary, -1)
        { Value = (object?)dto.ImagenData ?? DBNull.Value };
        var pDocData = new SqlParameter("@DocumentacionData", System.Data.SqlDbType.VarBinary, -1)
        { Value = (object?)dto.DocumentacionData ?? DBNull.Value };

        await _db.Database.ExecuteSqlRawAsync(
            @"EXEC sp_Dashboards_Crear 
              @Nombre, @Descripcion, @UnidadID, @URLReporte, @EsPrivado, @CreadoPorID,
              @ImagenURL, @ImagenData, @ImagenTipo,
              @DocumentacionData, @DocumentacionNombre, @DocumentacionTipo,
              @NuevoID OUTPUT, @Folio OUTPUT",
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@Descripcion", (object?)dto.Descripcion ?? DBNull.Value),
            new SqlParameter("@UnidadID", dto.UnidadID),
            new SqlParameter("@URLReporte", dto.URLReporte),
            new SqlParameter("@EsPrivado", dto.EsPrivado),
            new SqlParameter("@CreadoPorID", creadoPorId),
            new SqlParameter("@ImagenURL", (object?)dto.ImagenURL ?? DBNull.Value),
            pImagenData,
            new SqlParameter("@ImagenTipo", (object?)dto.ImagenTipo ?? DBNull.Value),
            pDocData,
            new SqlParameter("@DocumentacionNombre", (object?)dto.DocumentacionNombre ?? DBNull.Value),
            new SqlParameter("@DocumentacionTipo", (object?)dto.DocumentacionTipo ?? DBNull.Value),
            pId, pFolio);

        return ((int)pId.Value, (string)pFolio.Value);
    }

    public async Task ActualizarAsync(DashboardEditDto dto)
    {
        var pImagenData = new SqlParameter("@ImagenData", System.Data.SqlDbType.VarBinary, -1)
        { Value = (object?)dto.ImagenData ?? DBNull.Value };
        var pDocData = new SqlParameter("@DocumentacionData", System.Data.SqlDbType.VarBinary, -1)
        { Value = (object?)dto.DocumentacionData ?? DBNull.Value };

        await _db.Database.ExecuteSqlRawAsync(
            @"EXEC sp_Dashboards_Actualizar 
              @DashboardID, @Nombre, @Descripcion, @UnidadID, @URLReporte, 
              @EsPrivado, @Activo, @ImagenURL, @ImagenData, @ImagenTipo,
              @DocumentacionData, @DocumentacionNombre, @DocumentacionTipo, @EliminarDoc",
            new SqlParameter("@DashboardID", dto.DashboardID),
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@Descripcion", (object?)dto.Descripcion ?? DBNull.Value),
            new SqlParameter("@UnidadID", dto.UnidadID),
            new SqlParameter("@URLReporte", dto.URLReporte),
            new SqlParameter("@EsPrivado", dto.EsPrivado),
            new SqlParameter("@Activo", dto.Activo),
            new SqlParameter("@ImagenURL", (object?)dto.ImagenURL ?? DBNull.Value),
            pImagenData,
            new SqlParameter("@ImagenTipo", (object?)dto.ImagenTipo ?? DBNull.Value),
            pDocData,
            new SqlParameter("@DocumentacionNombre", (object?)dto.DocumentacionNombre ?? DBNull.Value),
            new SqlParameter("@DocumentacionTipo", (object?)dto.DocumentacionTipo ?? DBNull.Value),
            new SqlParameter("@EliminarDoc", dto.EliminarDocumentacion));
    }

    public async Task DesactivarAsync(int dashboardId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Dashboards_Desactivar @DashboardID",
            new SqlParameter("@DashboardID", dashboardId));

    public async Task<List<DashboardPermisoDto>> ObtenerPermisosAsync(int dashboardId)
        => await _db.Set<DashboardPermisoDto>()
            .FromSqlRaw("EXEC sp_DashPermisos_ObtenerPorDashboard @DashboardID",
                new SqlParameter("@DashboardID", dashboardId))
            .ToListAsync();

    public async Task OtorgarPermisoAsync(int dashboardId, int usuarioId, int otorgadoPorId)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_DashPermisos_Otorgar @DashboardID, @UsuarioID, @OtorgadoPorID, @NuevoID OUTPUT",
            new SqlParameter("@DashboardID", dashboardId),
            new SqlParameter("@UsuarioID", usuarioId),
            new SqlParameter("@OtorgadoPorID", otorgadoPorId),
            pId);
    }

    public async Task RevocarPermisoAsync(int dashboardId, int usuarioId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_DashPermisos_Revocar @DashboardID, @UsuarioID",
            new SqlParameter("@DashboardID", dashboardId),
            new SqlParameter("@UsuarioID", usuarioId));

    public async Task<bool> VerificarAccesoAsync(int dashboardId, int usuarioId)
    {
        var pAcceso = new SqlParameter("@TieneAcceso", System.Data.SqlDbType.Bit) { Direction = System.Data.ParameterDirection.Output };
        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_DashPermisos_VerificarAcceso @DashboardID, @UsuarioID, @TieneAcceso OUTPUT",
            new SqlParameter("@DashboardID", dashboardId),
            new SqlParameter("@UsuarioID", usuarioId),
            pAcceso);
        return (bool)pAcceso.Value;
    }

    public Task<List<UnidadNegocioDto>> ObtenerUnidadesAsync()
        => throw new NotImplementedException();
}

// ══════════════════════════════════════════════════════════════
//  UNIDAD NEGOCIO SERVICE
// ══════════════════════════════════════════════════════════════
public class UnidadNegocioService : IUnidadNegocioService
{
    private readonly CentralDashboardsContext _db;
    public UnidadNegocioService(CentralDashboardsContext db) => _db = db;

    public async Task<List<UnidadNegocioDto>> ObtenerTodasAsync(bool soloActivas = true)
        => await _db.Set<UnidadNegocioDto>()
            .FromSqlRaw("EXEC sp_UnidadesNegocio_ObtenerTodas @SoloActivas",
                new SqlParameter("@SoloActivas", soloActivas))
            .ToListAsync();

    public async Task<int> CrearAsync(UnidadNegocioCreateDto dto)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_UnidadesNegocio_Crear @Nombre, @Descripcion, @IconoURL, @NuevoID OUTPUT",
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@Descripcion", dto.Descripcion ?? (object)DBNull.Value),
            new SqlParameter("@IconoURL", dto.IconoURL ?? (object)DBNull.Value),
            pId);
        return (int)pId.Value;
    }

    public async Task ActualizarAsync(int id, UnidadNegocioCreateDto dto, bool activo)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_UnidadesNegocio_Actualizar @UnidadID, @Nombre, @Descripcion, @IconoURL, @Activo",
            new SqlParameter("@UnidadID", id),
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@Descripcion", dto.Descripcion ?? (object)DBNull.Value),
            new SqlParameter("@IconoURL", dto.IconoURL ?? (object)DBNull.Value),
            new SqlParameter("@Activo", activo));
}

// ══════════════════════════════════════════════════════════════
//  INCIDENCIA SERVICE
// ══════════════════════════════════════════════════════════════
public class IncidenciaService : IIncidenciaService
{
    private readonly CentralDashboardsContext _db;
    public IncidenciaService(CentralDashboardsContext db) => _db = db;

    public async Task<List<IncidenciaResumenDto>> ObtenerTodasAsync(int? dashboardId = null, int? usuarioId = null, int? estatusId = null)
        => await _db.Set<IncidenciaResumenDto>()
            .FromSqlRaw("EXEC sp_Incidencias_ObtenerTodas @DashboardID, @UsuarioID, @EstatusID",
                new SqlParameter("@DashboardID", dashboardId ?? (object)DBNull.Value),
                new SqlParameter("@UsuarioID", usuarioId ?? (object)DBNull.Value),
                new SqlParameter("@EstatusID", estatusId ?? (object)DBNull.Value))
            .ToListAsync();

    public async Task<IncidenciaDetalleDto?> ObtenerPorIdAsync(int incidenciaId)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC sp_Incidencias_ObtenerPorID @IncidenciaID";
        var p = cmd.CreateParameter();
        p.ParameterName = "@IncidenciaID";
        p.Value = incidenciaId;
        cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        int Ord(string col) => reader.GetOrdinal(col);
        bool IsNull(string col) => reader.IsDBNull(Ord(col));

        return new IncidenciaDetalleDto
        {
            IncidenciaID = reader.GetInt32(Ord("IncidenciaID")),
            Folio = reader.GetString(Ord("Folio")),
            Titulo = reader.GetString(Ord("Titulo")),
            Descripcion = reader.GetString(Ord("Descripcion")),
            Prioridad = reader.GetString(Ord("Prioridad")),
            NombreEstatus = reader.GetString(Ord("NombreEstatus")),
            ColorEstatus = IsNull("ColorEstatus") ? null : reader.GetString(Ord("ColorEstatus")),
            NombreDashboard = IsNull("NombreDashboard") ? null : reader.GetString(Ord("NombreDashboard")),
            FolioDashboard = IsNull("FolioDashboard") ? "" : reader.GetString(Ord("FolioDashboard")),
            TipoIncidencia = reader.GetString(Ord("TipoIncidencia")),
            ReportadoPor = reader.GetString(Ord("ReportadoPor")),
            AsignadoA = IsNull("AsignadoA") ? null : reader.GetString(Ord("AsignadoA")),
            FechaCreacion = reader.GetDateTime(Ord("FechaCreacion")),
            FechaResolucion = IsNull("FechaResolucion") ? null : reader.GetDateTime(Ord("FechaResolucion")),
            EstatusID = reader.GetInt32(Ord("EstatusID")),
            DashboardID = reader.GetInt32(Ord("DashboardID")),
            ReportadoPorID = reader.GetInt32(Ord("ReportadoPorID")),
            TipoIncidenciaID = reader.GetInt32(Ord("TipoIncidenciaID")),
            AsignadoAID = IsNull("AsignadoAID") ? null : reader.GetInt32(Ord("AsignadoAID")),
            AreaReportador = IsNull("AreaReportador") ? null : reader.GetString(Ord("AreaReportador")),
        };
    }

    public async Task<(int id, string folio)> CrearAsync(IncidenciaCreateDto dto, int reportadoPorId)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var pFolio = new SqlParameter("@Folio", System.Data.SqlDbType.NVarChar, 20) { Direction = System.Data.ParameterDirection.Output };

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Incidencias_Crear @DashboardID, @ReportadoPorID, @TipoIncidenciaID, @Titulo, @Descripcion, @Prioridad, @NuevoID OUTPUT, @Folio OUTPUT",
            new SqlParameter("@DashboardID", dto.DashboardID),
            new SqlParameter("@ReportadoPorID", reportadoPorId),
            new SqlParameter("@TipoIncidenciaID", dto.TipoIncidenciaID),
            new SqlParameter("@Titulo", dto.Titulo),
            new SqlParameter("@Descripcion", dto.Descripcion),
            new SqlParameter("@Prioridad", dto.Prioridad),
            pId, pFolio);

        return ((int)pId.Value, (string)pFolio.Value);
    }

    public async Task ActualizarAsync(IncidenciaEditDto dto)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Incidencias_Actualizar @IncidenciaID, @Titulo, @Descripcion, @Prioridad, @TipoIncidenciaID",
            new SqlParameter("@IncidenciaID", dto.IncidenciaID),
            new SqlParameter("@Titulo", dto.Titulo),
            new SqlParameter("@Descripcion", dto.Descripcion),
            new SqlParameter("@Prioridad", dto.Prioridad),
            new SqlParameter("@TipoIncidenciaID", dto.TipoIncidenciaID));

    public async Task CambiarEstatusAsync(int incidenciaId, int estatusId, int? asignadoAId = null)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Incidencias_CambiarEstatus @IncidenciaID, @EstatusID, @AsignadoAID",
            new SqlParameter("@IncidenciaID", incidenciaId),
            new SqlParameter("@EstatusID", estatusId),
            new SqlParameter("@AsignadoAID", asignadoAId ?? (object)DBNull.Value));

    public async Task EliminarAsync(int incidenciaId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Incidencias_Eliminar @IncidenciaID",
            new SqlParameter("@IncidenciaID", incidenciaId));
}

// ══════════════════════════════════════════════════════════════
//  SOLICITUD SERVICE
// ══════════════════════════════════════════════════════════════
public class SolicitudService : ISolicitudService
{
    private readonly CentralDashboardsContext _db;
    public SolicitudService(CentralDashboardsContext db) => _db = db;

    public async Task<List<SolicitudResumenDto>> ObtenerTodasAsync(int? dashboardId = null, int? usuarioId = null, int? estatusId = null)
        => await _db.Set<SolicitudResumenDto>()
            .FromSqlRaw("EXEC sp_Solicitudes_ObtenerTodas @DashboardID, @SolicitadoPorID, @EstatusID",
                new SqlParameter("@DashboardID", dashboardId ?? (object)DBNull.Value),
                new SqlParameter("@SolicitadoPorID", usuarioId ?? (object)DBNull.Value),
                new SqlParameter("@EstatusID", estatusId ?? (object)DBNull.Value))
            .ToListAsync();

    public async Task<SolicitudDetalleDto?> ObtenerPorIdAsync(int solicitudId)
        => (await _db.Set<SolicitudDetalleDto>()
            .FromSqlRaw("EXEC sp_Solicitudes_ObtenerPorID @SolicitudID",
                new SqlParameter("@SolicitudID", solicitudId))
            .ToListAsync()).FirstOrDefault();

    public async Task<(int id, string folio)> CrearAsync(SolicitudCreateDto dto, int solicitadoPorId)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var pFolio = new SqlParameter("@Folio", System.Data.SqlDbType.NVarChar, 20) { Direction = System.Data.ParameterDirection.Output };

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Solicitudes_Crear @DashboardID, @SolicitadoPorID, @TipoSolicitudID, @Titulo, @Descripcion, @Justificacion, @Prioridad, @NuevoID OUTPUT, @Folio OUTPUT",
            new SqlParameter("@DashboardID", dto.DashboardID ?? (object)DBNull.Value),
            new SqlParameter("@SolicitadoPorID", solicitadoPorId),
            new SqlParameter("@TipoSolicitudID", dto.TipoSolicitudID),
            new SqlParameter("@Titulo", dto.Titulo),
            new SqlParameter("@Descripcion", dto.Descripcion),
            new SqlParameter("@Justificacion", dto.Justificacion ?? (object)DBNull.Value),
            new SqlParameter("@Prioridad", dto.Prioridad),
            pId, pFolio);

        return ((int)pId.Value, (string)pFolio.Value);
    }

    public async Task ActualizarAsync(SolicitudEditDto dto)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Solicitudes_Actualizar @SolicitudID, @Titulo, @Descripcion, @Justificacion, @TipoSolicitudID, @EstatusID, @AsignadoAID, @Prioridad",
            new SqlParameter("@SolicitudID", dto.SolicitudID),
            new SqlParameter("@Titulo", dto.Titulo),
            new SqlParameter("@Descripcion", dto.Descripcion),
            new SqlParameter("@Justificacion", dto.Justificacion ?? (object)DBNull.Value),
            new SqlParameter("@TipoSolicitudID", dto.TipoSolicitudID),
            new SqlParameter("@EstatusID", dto.EstatusID),
            new SqlParameter("@AsignadoAID", dto.AsignadoAID ?? (object)DBNull.Value),
            new SqlParameter("@Prioridad", dto.Prioridad));

    public async Task CambiarEstatusAsync(int solicitudId, int estatusId, int? asignadoAId = null)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Solicitudes_CambiarEstatus @SolicitudID, @EstatusID, @AsignadoAID",
            new SqlParameter("@SolicitudID", solicitudId),
            new SqlParameter("@EstatusID", estatusId),
            new SqlParameter("@AsignadoAID", asignadoAId ?? (object)DBNull.Value));

    public async Task EliminarAsync(int solicitudId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Solicitudes_Eliminar @SolicitudID",
            new SqlParameter("@SolicitudID", solicitudId));

    public async Task<HashSet<int>> ObtenerDashboardsYaSolicitadosAsync(int usuarioId)
    {
        var lista = await _db.Solicitudes
            .Where(s => s.SolicitadoPorID == usuarioId
                     && s.TipoSolicitudID == 3
                     && s.DashboardID.HasValue)
            .Select(s => s.DashboardID!.Value)
            .ToListAsync();
        return lista.ToHashSet();
    }
}

// ══════════════════════════════════════════════════════════════
//  COMENTARIO SERVICE
// ══════════════════════════════════════════════════════════════
public class ComentarioService : IComentarioService
{
    private readonly CentralDashboardsContext _db;
    public ComentarioService(CentralDashboardsContext db) => _db = db;

    public async Task<List<ComentarioDto>> ObtenerPorTicketAsync(string tipoRegistro, int registroId)
        => await _db.Set<ComentarioDto>()
            .FromSqlRaw("EXEC sp_Comentarios_ObtenerPorTicket @TipoRegistro, @RegistroID",
                new SqlParameter("@TipoRegistro", tipoRegistro),
                new SqlParameter("@RegistroID", registroId))
            .ToListAsync();

    public async Task<int> AgregarAsync(string tipoRegistro, int registroId, int autorId, bool esAdmin, string mensaje)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Comentarios_Agregar @TipoRegistro, @RegistroID, @AutorID, @EsRespuestaAdmin, @Mensaje, @ComentarioPadreID, @NuevoID OUTPUT",
            new SqlParameter("@TipoRegistro", tipoRegistro),
            new SqlParameter("@RegistroID", registroId),
            new SqlParameter("@AutorID", autorId),
            new SqlParameter("@EsRespuestaAdmin", esAdmin),
            new SqlParameter("@Mensaje", mensaje),
            new SqlParameter("@ComentarioPadreID", DBNull.Value),
            pId);
        return (int)pId.Value;
    }

    public async Task EliminarAsync(int comentarioId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Comentarios_Eliminar @ComentarioID",
            new SqlParameter("@ComentarioID", comentarioId));
}

// ══════════════════════════════════════════════════════════════
//  USUARIO SERVICE
// ══════════════════════════════════════════════════════════════
public class UsuarioService : IUsuarioService
{
    private readonly CentralDashboardsContext _db;
    public UsuarioService(CentralDashboardsContext db) => _db = db;

    public async Task<List<UsuarioDto>> ObtenerTodosAsync(bool soloActivos = true)
        => await _db.Set<UsuarioDto>()
            .FromSqlRaw("EXEC sp_Usuarios_ObtenerTodos @SoloActivos",
                new SqlParameter("@SoloActivos", soloActivos))
            .ToListAsync();

    public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
        => (await _db.Set<UsuarioDto>()
            .FromSqlRaw("EXEC sp_Usuarios_ObtenerPorID @UsuarioID",
                new SqlParameter("@UsuarioID", id))
            .ToListAsync()).FirstOrDefault();

    public async Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo)
        => await _db.Usuarios
            .Where(u => u.Correo == correo && u.Activo)
            .Join(_db.Roles, u => u.RolID, r => r.RolID, (u, r) => new UsuarioDto
            {
                UsuarioID = u.UsuarioID,
                Nombre = u.Nombre,
                Correo = u.Correo,
                RolID = u.RolID,
                NombreRol = r.NombreRol,
                Activo = u.Activo
            }).FirstOrDefaultAsync();

    // CORRECCIÓN: ObtenerHashAsync eliminado — ContrasenaHash ya no existe en la tabla Usuarios.
    // Si algún archivo lo llama, reemplaza la llamada por null directamente.

    public async Task<int> CrearAsync(UsuarioCreateDto dto)
    {
        var pId = new SqlParameter("@NuevoID", System.Data.SqlDbType.Int)
        { Direction = System.Data.ParameterDirection.Output };

        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Usuarios_Crear @Nombre, @Correo, @RolID, @AreaID, @NuevoID OUTPUT",
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@Correo", dto.Correo),
            new SqlParameter("@RolID", dto.RolID),
            new SqlParameter("@AreaID", dto.AreaID ?? (object)DBNull.Value),
            pId);

        return (int)pId.Value;
    }

    public async Task ActualizarAsync(UsuarioEditDto dto)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Usuarios_Actualizar @UsuarioID, @Nombre, @RolID, @AreaID, @Activo",
            new SqlParameter("@UsuarioID", dto.UsuarioID),
            new SqlParameter("@Nombre", dto.Nombre),
            new SqlParameter("@RolID", dto.RolID),
            new SqlParameter("@AreaID", dto.AreaID ?? (object)DBNull.Value),
            new SqlParameter("@Activo", dto.Activo));

    public async Task DesactivarAsync(int id)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Usuarios_Desactivar @UsuarioID",
            new SqlParameter("@UsuarioID", id));

    public async Task ActualizarUltimoAccesoAsync(int id)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Usuarios_ActualizarUltimoAcceso @UsuarioID",
            new SqlParameter("@UsuarioID", id));

    public async Task<List<RolDto>> ObtenerRolesAsync()
        => await _db.Roles
            .Select(r => new RolDto { RolID = r.RolID, NombreRol = r.NombreRol, Descripcion = r.Descripcion })
            .ToListAsync();

    public async Task<List<UsuarioDto>> ObtenerAdminsAsync()
        => await _db.Usuarios
            .Where(u => u.Activo)
            .Join(_db.Roles, u => u.RolID, r => r.RolID, (u, r) => new { u, r })
            .Where(x => x.r.NombreRol == "Administrador")
            .Select(x => new UsuarioDto
            {
                UsuarioID = x.u.UsuarioID,
                Nombre = x.u.Nombre,
                Correo = x.u.Correo,
                RolID = x.u.RolID,
                NombreRol = x.r.NombreRol,
                Activo = x.u.Activo
            }).ToListAsync();
}

// ══════════════════════════════════════════════════════════════
//  AUDITORIA SERVICE
// ══════════════════════════════════════════════════════════════
public class AuditoriaService : IAuditoriaService
{
    private readonly CentralDashboardsContext _db;
    public AuditoriaService(CentralDashboardsContext db) => _db = db;

    public async Task RegistrarAsync(int usuarioId, string accion, string? tabla = null, int? registroId = null, string? detalle = null, string? ip = null)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Auditoria_Registrar @UsuarioID, @Accion, @TablaAfectada, @RegistroID, @Detalle, @IPOrigen",
            new SqlParameter("@UsuarioID", usuarioId),
            new SqlParameter("@Accion", accion),
            new SqlParameter("@TablaAfectada", tabla ?? (object)DBNull.Value),
            new SqlParameter("@RegistroID", registroId ?? (object)DBNull.Value),
            new SqlParameter("@Detalle", detalle ?? (object)DBNull.Value),
            new SqlParameter("@IPOrigen", ip ?? (object)DBNull.Value));

    public async Task<List<AuditoriaDto>> ObtenerRecentesAsync(int top = 100)
        => await _db.Set<AuditoriaDto>()
            .FromSqlRaw("EXEC sp_Auditoria_ObtenerRecientes @Top",
                new SqlParameter("@Top", top))
            .ToListAsync();

    public async Task<List<AuditoriaDto>> ObtenerPorUsuarioAsync(int usuarioId, int top = 50)
        => await _db.Set<AuditoriaDto>()
            .FromSqlRaw("EXEC sp_Auditoria_ObtenerPorUsuario @UsuarioID, @Top",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Top", top))
            .ToListAsync();
}

// ══════════════════════════════════════════════════════════════
//  NOTIFICACION SERVICE
// ══════════════════════════════════════════════════════════════
public class NotificacionService : INotificacionService
{
    private readonly CentralDashboardsContext _db;
    public NotificacionService(CentralDashboardsContext db) => _db = db;

    public async Task CrearAsync(int usuarioId, string titulo, string mensaje, string tipo,
                                  string? iconoClass = null, string? colorClass = null, string? url = null)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Notificaciones_Crear @UsuarioID, @Titulo, @Mensaje, @Tipo, @IconoClass, @ColorClass, @Url",
            new SqlParameter("@UsuarioID", usuarioId),
            new SqlParameter("@Titulo", titulo),
            new SqlParameter("@Mensaje", mensaje),
            new SqlParameter("@Tipo", tipo),
            new SqlParameter("@IconoClass", iconoClass ?? (object)DBNull.Value),
            new SqlParameter("@ColorClass", colorClass ?? (object)DBNull.Value),
            new SqlParameter("@Url", url ?? (object)DBNull.Value));

    public async Task<List<NotificacionDto>> ObtenerPorUsuarioAsync(int usuarioId, int top = 20)
        => await _db.Set<NotificacionDto>()
            .FromSqlRaw("EXEC sp_Notificaciones_ObtenerPorUsuario @UsuarioID, @Top",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Top", top))
            .ToListAsync();

    public async Task<int> ContarNoLeidasAsync(int usuarioId)
    {
        var p = new SqlParameter("@NoLeidas", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Notificaciones_ContarNoLeidas @UsuarioID, @NoLeidas OUTPUT",
            new SqlParameter("@UsuarioID", usuarioId), p);
        return (int)p.Value;
    }

    public async Task MarcarLeidaAsync(int notificacionId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Notificaciones_MarcarLeida @NotificacionID",
            new SqlParameter("@NotificacionID", notificacionId));

    public async Task MarcarTodasLeidasAsync(int usuarioId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Notificaciones_MarcarTodasLeidas @UsuarioID",
            new SqlParameter("@UsuarioID", usuarioId));

    public async Task NotificarNuevaIncidenciaAsync(int adminId, string folio, string titulo, int incidenciaId, string nombreUsuario)
        => await CrearAsync(adminId,
            "Nueva incidencia registrada",
            $"{nombreUsuario} reportó \"{titulo}\" ({folio})",
            "Incidencia", "bi bi-exclamation-triangle-fill", "danger",
            $"/Incidencias/Detail?id={incidenciaId}");

    public async Task NotificarNuevaSolicitudAsync(int adminId, string folio, string titulo, int solicitudId, string nombreUsuario)
        => await CrearAsync(adminId,
            "Nueva solicitud registrada",
            $"{nombreUsuario} creó \"{titulo}\" ({folio})",
            "Solicitud", "bi bi-clipboard-plus-fill", "warning",
            $"/Solicitudes/Detail?id={solicitudId}");

    public async Task NotificarCambioEstatusIncidenciaAsync(int usuarioId, string folio, string nuevoEstatus, int incidenciaId, string nombreAdmin = "Un administrador")
        => await CrearAsync(usuarioId,
            $"Incidencia {folio} actualizada",
            $"{nombreAdmin} cambió el estatus a \"{nuevoEstatus}\"",
            "Incidencia", "bi bi-arrow-repeat", "info",
            $"/Incidencias/Detail?id={incidenciaId}");

    public async Task NotificarCambioEstatusSolicitudAsync(int usuarioId, string folio, string nuevoEstatus, int solicitudId, string nombreAdmin = "Un administrador")
        => await CrearAsync(usuarioId,
            $"Solicitud {folio} actualizada",
            $"{nombreAdmin} cambió el estatus a \"{nuevoEstatus}\"",
            "Solicitud", "bi bi-arrow-repeat", "info",
            $"/Solicitudes/Detail?id={solicitudId}");

    public async Task NotificarNuevoComentarioAsync(int usuarioId, string folio, string tipoTicket, int registroId, string nombreAutor)
    {
        var esIncidencia = tipoTicket == "Incidencia";
        var url = esIncidencia ? $"/Incidencias/Detail?id={registroId}" : $"/Solicitudes/Detail?id={registroId}";
        var tipo = esIncidencia ? "la incidencia" : "la solicitud";
        await CrearAsync(usuarioId,
            $"Nuevo comentario en {folio}",
            $"{nombreAutor} comentó en {tipo} {folio}",
            "Comentario", "bi bi-chat-dots-fill", "info", url);
    }

    public async Task NotificarPermisoOtorgadoAsync(int usuarioId, string nombreDashboard, int dashboardId)
        => await CrearAsync(usuarioId,
            "Acceso otorgado",
            $"Ahora tienes acceso al dashboard \"{nombreDashboard}\"",
            "Permiso", "bi bi-shield-check-fill", "success",
            $"/Dashboards/Ver?DashboardId={dashboardId}");

    public async Task NotificarPermisoRevocadoAsync(int usuarioId, string nombreDashboard)
        => await CrearAsync(usuarioId,
            "Acceso revocado",
            $"Se revocó tu acceso al dashboard \"{nombreDashboard}\"",
            "Permiso", "bi bi-shield-x-fill", "danger", null);

    public async Task NotificarNuevoAvisoAsync(string titulo, int avisoId, string? nombreAdmin)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Notificaciones_CrearParaTodos @Titulo, @Mensaje, @Tipo, @IconoClass, @ColorClass, @Url",
            new SqlParameter("@Titulo", "📢 Nuevo aviso publicado"),
            new SqlParameter("@Mensaje", $"{nombreAdmin ?? "Un administrador"} publicó \"{titulo}\""),
            new SqlParameter("@Tipo", "Aviso"),
            new SqlParameter("@IconoClass", "bi bi-megaphone-fill"),
            new SqlParameter("@ColorClass", "success"),
            new SqlParameter("@Url", $"/Admin/Avisos/Detalle?id={avisoId}"));
}

// ══════════════════════════════════════════════════════════════
//  AVISO SERVICE
// ══════════════════════════════════════════════════════════════
public class AvisoService : IAvisoService
{
    private readonly CentralDashboardsContext _db;
    public AvisoService(CentralDashboardsContext db) => _db = db;

    public async Task<List<AvisoDto>> ObtenerTodosAsync(bool soloActivos = true)
        => await _db.Set<AvisoDto>()
            .FromSqlRaw("EXEC sp_Avisos_Listar @SoloActivos",
                new SqlParameter("@SoloActivos", soloActivos))
            .ToListAsync();

    public async Task<AvisoDto?> ObtenerPorIdAsync(int id)
        => (await _db.Set<AvisoDto>()
            .FromSqlRaw("EXEC sp_Avisos_Obtener @Id",
                new SqlParameter("@Id", id))
            .ToListAsync()).FirstOrDefault();

    public async Task<int> CrearAsync(AvisoCreateDto dto, int creadoPorId)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC sp_Avisos_Crear @Titulo, @Contenido, @Tipo, @Fecha, @CreadoPor";

        void AddParam(string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        AddParam("@Titulo", dto.Titulo);
        AddParam("@Contenido", dto.Contenido);
        AddParam("@Tipo", dto.Tipo);
        AddParam("@Fecha", dto.Fecha);
        AddParam("@CreadoPor", creadoPorId);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task ActualizarAsync(AvisoEditDto dto)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Avisos_Actualizar @Id, @Titulo, @Contenido, @Tipo, @Fecha, @Activo",
            new SqlParameter("@Id", dto.Id),
            new SqlParameter("@Titulo", dto.Titulo),
            new SqlParameter("@Contenido", dto.Contenido),
            new SqlParameter("@Tipo", dto.Tipo),
            new SqlParameter("@Fecha", dto.Fecha),
            new SqlParameter("@Activo", dto.Activo));

    public async Task CambiarEstadoAsync(int id, bool activo)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Avisos_CambiarEstado @Id, @Activo",
            new SqlParameter("@Id", id),
            new SqlParameter("@Activo", activo));

    public async Task EliminarAsync(int id)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Avisos_Eliminar @Id",
            new SqlParameter("@Id", id));

    public async Task<List<AvisoComentarioDto>> ObtenerComentariosAsync(int avisoId)
        => await _db.Set<AvisoComentarioDto>()
            .FromSqlRaw("EXEC sp_AvisosComentarios_Listar @AvisoID",
                new SqlParameter("@AvisoID", avisoId))
            .ToListAsync();

    public async Task<int> AgregarComentarioAsync(int avisoId, int usuarioId, string mensaje)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC sp_AvisosComentarios_Crear @AvisoID, @UsuarioID, @Mensaje";

        void AddParam(string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        AddParam("@AvisoID", avisoId);
        AddParam("@UsuarioID", usuarioId);
        AddParam("@Mensaje", mensaje);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task EliminarComentarioAsync(int comentarioId)
        => await _db.Database.ExecuteSqlRawAsync(
            "EXEC sp_AvisosComentarios_Eliminar @ComentarioID",
            new SqlParameter("@ComentarioID", comentarioId));

    public async Task<List<AvisoReaccionDto>> ObtenerReaccionesAsync(int avisoId, int usuarioId)
        => await _db.Set<AvisoReaccionDto>()
            .FromSqlRaw("EXEC sp_AvisosReacciones_Obtener @AvisoID, @UsuarioID",
                new SqlParameter("@AvisoID", avisoId),
                new SqlParameter("@UsuarioID", usuarioId))
            .ToListAsync();

    public async Task<List<AvisoReaccionDto>> ToggleReaccionAsync(int avisoId, int usuarioId, string tipo)
        => await _db.Set<AvisoReaccionDto>()
            .FromSqlRaw("EXEC sp_AvisosReacciones_Toggle @AvisoID, @UsuarioID, @Tipo",
                new SqlParameter("@AvisoID", avisoId),
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Tipo", tipo))
            .ToListAsync();
}