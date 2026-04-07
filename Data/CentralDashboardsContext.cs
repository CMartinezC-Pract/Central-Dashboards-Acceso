using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CentralDashboards.Data;


public class CentralDashboardsContext : DbContext
{
    public CentralDashboardsContext(DbContextOptions<CentralDashboardsContext> options)
        : base(options) { }

    // ── Tablas principales ───────────────────────────────────
    public DbSet<RolEntity> Roles { get; set; }
    public DbSet<AreaEntity> Areas { get; set; }
    public DbSet<UsuarioEntity> Usuarios { get; set; }
    public DbSet<DashboardEntity> Dashboards { get; set; }
    public DbSet<DashboardPermisoEntity> DashboardPermisos { get; set; }
    public DbSet<EstatusEntity> Estatus { get; set; }
    public DbSet<TipoIncidenciaEntity> TiposIncidencia { get; set; }
    public DbSet<TipoSolicitudEntity> TiposSolicitud { get; set; }
    public DbSet<IncidenciaEntity> Incidencias { get; set; }
    public DbSet<SolicitudEntity> Solicitudes { get; set; }
    public DbSet<ComentarioEntity> Comentarios { get; set; }
    public DbSet<AuditoriaEntity> Auditoria { get; set; }

    // ── Keyless para resultados de SP ────────────────────────


    public DbSet<UnidadNegocioEntity> UnidadesNegocio { get; set; }


    // ── Keyless para resultados de SP ────────────────────────
    public DbSet<IncidenciaResumenDto> IncidenciasResumen { get; set; }
    

    public DbSet<SolicitudResumenDto> SolicitudesResumen { get; set; }
    public DbSet<SolicitudDetalleDto> SolicitudesDetalle { get; set; }
    public DbSet<DashboardDto> DashboardsDto { get; set; }
    public DbSet<ComentarioDto> ComentariosDto { get; set; }
    public DbSet<AuditoriaDto> AuditoriasDto { get; set; }
    public DbSet<DashboardPermisoDto> PermisosDto { get; set; }
    public DbSet<UsuarioDto> UsuariosDto { get; set; }
    public DbSet<AreaDto> AreasDto { get; set; }
    public DbSet<CentralDashboards.Models.Entities.TicketImagenEntity> TicketImagenes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entidades con clave primaria
        modelBuilder.Entity<RolEntity>().ToTable("Roles").HasKey(e => e.RolID);
        modelBuilder.Entity<AreaEntity>().ToTable("Areas").HasKey(e => e.AreaID);
        modelBuilder.Entity<UsuarioEntity>().ToTable("Usuarios").HasKey(e => e.UsuarioID);


        modelBuilder.Entity<DashboardEntity>(e =>
        {
            e.ToTable("Dashboards").HasKey(x => x.DashboardID);
            e.Property(x => x.ImagenData).IsRequired(false);
            e.Property(x => x.DocumentacionData).IsRequired(false);
        });

        modelBuilder.Entity<UnidadNegocioEntity>()
    .ToTable("UnidadesNegocio")
    .HasKey(e => e.UnidadID);


        modelBuilder.Entity<DashboardPermisoEntity>().ToTable("DashboardPermisos").HasKey(e => e.PermisoID);
        modelBuilder.Entity<EstatusEntity>().ToTable("Estatus").HasKey(e => e.EstatusID);
        modelBuilder.Entity<TipoIncidenciaEntity>().ToTable("TiposIncidencia").HasKey(e => e.TipoID);
        modelBuilder.Entity<TipoSolicitudEntity>().ToTable("TiposSolicitud").HasKey(e => e.TipoID);
        modelBuilder.Entity<IncidenciaEntity>().ToTable("Incidencias").HasKey(e => e.IncidenciaID);
        modelBuilder.Entity<SolicitudEntity>().ToTable("Solicitudes").HasKey(e => e.SolicitudID);
        modelBuilder.Entity<ComentarioEntity>().ToTable("Comentarios").HasKey(e => e.ComentarioID);
        modelBuilder.Entity<AuditoriaEntity>().ToTable("Auditoria").HasKey(e => e.AuditoriaID);
        modelBuilder.Entity<AvisoReaccionDto>().HasNoKey().ToView(null);

        //
        modelBuilder.Entity<AvisoDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AvisoComentarioDto>().HasNoKey().ToView(null);
        ///


        modelBuilder.Entity<UnidadNegocioDto>().HasNoKey();

        // DTOs sin clave (resultados de SP)
        modelBuilder.Entity<IncidenciaResumenDto>().HasNoKey();
        modelBuilder.Entity<NotificacionDto>().HasNoKey();

        modelBuilder.Entity<SolicitudResumenDto>().HasNoKey();
        modelBuilder.Entity<SolicitudDetalleDto>().HasNoKey();
        modelBuilder.Entity<DashboardDto>().HasNoKey();
        modelBuilder.Entity<ComentarioDto>().HasNoKey();
        modelBuilder.Entity<AuditoriaDto>().HasNoKey();
        modelBuilder.Entity<DashboardPermisoDto>().HasNoKey();
        modelBuilder.Entity<UsuarioDto>().HasNoKey();
        modelBuilder.Entity<AreaDto>().HasNoKey();
    }
}

// ══════════════════════════════════════════════════════════════
//  ENTIDADES (mapean directamente a tablas SQL)
// ══════════════════════════════════════════════════════════════
public class RolEntity
{
    public int RolID { get; set; }
    public string NombreRol { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class AreaEntity
{
    public int AreaID { get; set; }
    public string NombreArea { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? IconoURL { get; set; }
    public bool Activo { get; set; }
}

public class UsuarioEntity
{
    public int UsuarioID { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string ContrasenaHash { get; set; } = "";
    public int RolID { get; set; }
    public int? AreaID { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

// Reemplaza la clase DashboardEntity en AppDbContext.cs
// Cambios: AreaID → UnidadID, agregadas ImagenURL, ImagenData, ImagenTipo


public class DashboardEntity
{
    public int DashboardID { get; set; }
    public string Folio { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public int UnidadID { get; set; }
    public string URLReporte { get; set; } = "";
    public bool EsPrivado { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public int CreadoPorID { get; set; }
    public bool Activo { get; set; }
    public string? ImagenURL { get; set; }
    public string? ImagenTipo { get; set; }
    public string? DocumentacionNombre { get; set; }
    public string? DocumentacionTipo { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public byte[]? ImagenData { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public byte[]? DocumentacionData { get; set; }
}


public class DashboardPermisoEntity
{
    public int PermisoID { get; set; }
    public int DashboardID { get; set; }
    public int UsuarioID { get; set; }
    public DateTime FechaOtorgado { get; set; }
    public int OtorgadoPorID { get; set; }
}

public class EstatusEntity
{
    public int EstatusID { get; set; }
    public string NombreEstatus { get; set; } = "";
    public string AplicaA { get; set; } = "";
    public string? Color { get; set; }
}

public class TipoIncidenciaEntity
{
    public int TipoID { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class TipoSolicitudEntity
{
    public int TipoID { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class IncidenciaEntity
{
    public int IncidenciaID { get; set; }
    public string Folio { get; set; } = "";
    public int DashboardID { get; set; }
    public int ReportadoPorID { get; set; }
    public int TipoIncidenciaID { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Prioridad { get; set; } = "Media";
    public int EstatusID { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public int? AsignadoAID { get; set; }
}

public class SolicitudEntity
{
    public int SolicitudID { get; set; }
    public string Folio { get; set; } = "";
    public int? DashboardID { get; set; }
    public int SolicitadoPorID { get; set; }
    public int TipoSolicitudID { get; set; }
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string? Justificacion { get; set; }
    public int EstatusID { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public int? AsignadoAID { get; set; }
}

public class ComentarioEntity
{
    public int ComentarioID { get; set; }
    public string TipoRegistro { get; set; } = "";
    public int RegistroID { get; set; }
    public int AutorID { get; set; }
    public bool EsRespuestaAdmin { get; set; }
    public string Mensaje { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public int? ComentarioPadreID { get; set; }
}

public class AuditoriaEntity
{
    public int AuditoriaID { get; set; }
    public int UsuarioID { get; set; }
    public string Accion { get; set; } = "";
    public string? TablaAfectada { get; set; }
    public int? RegistroID { get; set; }
    public string? Detalle { get; set; }
    public DateTime FechaAccion { get; set; }
    public string? IPOrigen { get; set; }
}

// ─────────────────────────────────────────────────────────────
//  DashboardCreateDto  — agrega esto en tu archivo Dtos.cs
//  ImagenData es byte[]? para que sea completamente opcional
// ─────────────────────────────────────────────────────────────



public class UnidadNegocioEntity
{
    public int UnidadID { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? IconoURL { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
