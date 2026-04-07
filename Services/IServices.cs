/*using CentralDashboards.Models.Dtos;
using CentralDashboards.Data;

namespace CentralDashboards.Services;

public interface IDashboardService
{
    Task<List<AreaDto>>            ObtenerAreasActivasAsync();
    Task<List<DashboardDto>>       ObtenerPorAreaAsync(int areaId, int usuarioId);
    Task<List<DashboardDto>>       ObtenerTodosAsync(int? areaId = null, bool soloActivos = true);
    Task<DashboardDto?>            ObtenerPorIdAsync(int dashboardId);
    Task<(int id, string folio)>   CrearAsync(DashboardCreateDto dto, int creadoPorId);
    Task                           ActualizarAsync(DashboardEditDto dto);
    Task                           DesactivarAsync(int dashboardId);
    Task<List<DashboardPermisoDto>> ObtenerPermisosAsync(int dashboardId);
    Task                           OtorgarPermisoAsync(int dashboardId, int usuarioId, int otorgadoPorId);
    Task                           RevocarPermisoAsync(int dashboardId, int usuarioId);
    Task<bool>                     VerificarAccesoAsync(int dashboardId, int usuarioId);
}

public interface IIncidenciaService
{
    Task<List<IncidenciaResumenDto>> ObtenerTodasAsync(int? dashboardId = null, int? usuarioId = null, int? estatusId = null);
    Task<IncidenciaDetalleDto?>      ObtenerPorIdAsync(int incidenciaId);
    Task<(int id, string folio)>     CrearAsync(IncidenciaCreateDto dto, int reportadoPorId);
    Task                             ActualizarAsync(IncidenciaEditDto dto);
    Task                             CambiarEstatusAsync(int incidenciaId, int estatusId, int? asignadoAId = null);
    Task                             EliminarAsync(int incidenciaId);
}

public interface ISolicitudService
{
    Task<List<SolicitudResumenDto>> ObtenerTodasAsync(int? dashboardId = null, int? usuarioId = null, int? estatusId = null);
    Task<SolicitudDetalleDto?>      ObtenerPorIdAsync(int solicitudId);
    Task<(int id, string folio)>    CrearAsync(SolicitudCreateDto dto, int solicitadoPorId);
    Task                            ActualizarAsync(SolicitudEditDto dto);
    Task                            CambiarEstatusAsync(int solicitudId, int estatusId, int? asignadoAId = null);
    Task                            EliminarAsync(int solicitudId);
}

public interface IComentarioService
{
    Task<List<ComentarioDto>> ObtenerPorTicketAsync(string tipoRegistro, int registroId);
    Task<int>                 AgregarAsync(string tipoRegistro, int registroId, int autorId, bool esAdmin, string mensaje);
    Task                      EliminarAsync(int comentarioId);
}

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ObtenerTodosAsync(bool soloActivos = true);
    Task<UsuarioDto?> ObtenerPorIdAsync(int id);
    Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo);
    Task<string?> ObtenerHashAsync(string correo); // Agregado
    Task<int> CrearAsync(UsuarioCreateDto dto);
    Task ActualizarAsync(UsuarioEditDto dto);
    Task DesactivarAsync(int id);
    Task ActualizarUltimoAccesoAsync(int id); // Agregado
    Task<List<RolDto>> ObtenerRolesAsync();
}

public interface IAuditoriaService
{
    Task                     RegistrarAsync(int usuarioId, string accion, string? tabla = null, int? registroId = null, string? detalle = null, string? ip = null);
    Task<List<AuditoriaDto>> ObtenerRecentesAsync(int top = 100);
    Task<List<AuditoriaDto>> ObtenerPorUsuarioAsync(int usuarioId, int top = 50);
}
*/