using System.ComponentModel.DataAnnotations;
namespace CentralDashboards.Models.Dtos;
// ══════════════════════════════════════════════════════════════


// ══════════════════════════════════════════════════════════════
//  UNIDADES DE NEGOCIO
// ══════════════════════════════════════════════════════════════
public class UnidadNegocioDto
{
    public int UnidadID { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? IconoURL { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class UnidadNegocioCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? IconoURL { get; set; }
    public bool Activo { get; set; } = true;  // ← agrega esta línea

}

// ══════════════════════════════════════════════════════════════
//  ÁREAS
// ══════════════════════════════════════════════════════════════
public class AreaDto
{
    public int AreaID { get; set; }
    public string NombreArea { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? IconoURL { get; set; }
    public bool Activo { get; set; }
}

public class AreaCreateDto
{
    [Required(ErrorMessage = "El nombre del área es obligatorio.")]
    [MaxLength(100)]
    public string NombreArea { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? IconoURL { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  DASHBOARDS
// ══════════════════════════════════════════════════════════════
public class DashboardDto
{
    public int DashboardID { get; set; }
    public string Folio { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string URLReporte { get; set; } = "";
    public bool EsPrivado { get; set; }
    public bool Activo { get; set; }
    public int UnidadID { get; set; }   // ← antes AreaID
    public string NombreUnidad { get; set; } = ""; // ← antes NombreArea
    public string CreadoPor { get; set; } = "";
    public DateTime FechaPublicacion { get; set; }
    public bool TieneAcceso { get; set; } = true;
    public string? ImagenPrincipal { get; set; }

    // ── Documentación ─────────────────────────────────────────
    public bool TieneDocumentacion { get; set; }
    public string? DocumentacionNombre { get; set; }
    public string? DocumentacionTipo { get; set; }
}

public class DashboardCreateDto
{
    [Required, MaxLength(150)]
    public string Nombre { get; set; } = "";

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    public int UnidadID { get; set; }

    [Required, MaxLength(500)]
    public string URLReporte { get; set; } = "";

    public bool EsPrivado { get; set; }

    // Imagen — todo opcional
    public string? ImagenURL { get; set; }   // URL externa (si pegan link)
    public byte[]? ImagenData { get; set; }   // Binario (si suben archivo)
    public string? ImagenTipo { get; set; }   // MIME type ej: "image/png"

    // ── Documentación ─────────────────────────────────────────
    public byte[]? DocumentacionData { get; set; }
    public string? DocumentacionNombre { get; set; }
    public string? DocumentacionTipo { get; set; }
    public bool EliminarDocumentacion { get; set; }

}

public class DashboardEditDto : DashboardCreateDto
{
    public int DashboardID { get; set; }
    public bool Activo { get; set; }
}

public class DashboardPermisoDto
{
    public int PermisoID { get; set; }
    public int UsuarioID { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string OtorgadoPor { get; set; } = "";
    public DateTime FechaOtorgado { get; set; }
    public string? NombreRol { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  USUARIOS
// ══════════════════════════════════════════════════════════════
public class UsuarioDto
{
    public int UsuarioID { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public bool Activo { get; set; }
    public string NombreRol { get; set; } = "";
    public string? NombreArea { get; set; }
    public int RolID { get; set; }
    public int? AreaID { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

public class UsuarioCreateDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    // ← ELIMINADO: public string Contrasena { get; set; }

    [Required]
    public int RolID { get; set; }

    public int? AreaID { get; set; }
}
public class UsuarioEditDto
{
    public int UsuarioID { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public int RolID { get; set; }
    public int? AreaID { get; set; }
    public bool Activo { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  ESTATUS Y CATÁLOGOS
// ══════════════════════════════════════════════════════════════
public class EstatusDto
{
    public int EstatusID { get; set; }
    public string NombreEstatus { get; set; } = "";
    public string AplicaA { get; set; } = "";
    public string? Color { get; set; }
}

public class TipoDto
{
    public int TipoID { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class RolDto
{
    public int RolID { get; set; }
    public string NombreRol { get; set; } = "";
    public string? Descripcion { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  INCIDENCIAS
// ══════════════════════════════════════════════════════════════
public class IncidenciaResumenDto
{
    public int IncidenciaID { get; set; }
    public string Folio { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string NombreEstatus { get; set; } = "";
    public string? ColorEstatus { get; set; }
    public string FolioDashboard { get; set; } = "";
    public string NombreDashboard { get; set; } = "";
    public string TipoIncidencia { get; set; } = "";
    public string ReportadoPor { get; set; } = "";
    public string? AsignadoA { get; set; }
}

public class IncidenciaDetalleDto : IncidenciaResumenDto
{
    public string Descripcion { get; set; } = "";
    public int EstatusID { get; set; }
    public int DashboardID { get; set; }
    public int ReportadoPorID { get; set; }
    public int TipoIncidenciaID { get; set; }
    public int? AsignadoAID { get; set; }
    public string? AreaReportador { get; set; }   // ← NUEVO

}

public class IncidenciaCreateDto
{
    [Required(ErrorMessage = "Selecciona el Dashboard afectado.")]
    public int DashboardID { get; set; }

    [Required(ErrorMessage = "Selecciona el tipo de incidencia.")]
    public int TipoIncidenciaID { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Descripcion { get; set; } = "";

    public string Prioridad { get; set; } = "Media";
}

public class IncidenciaEditDto
{
    public int IncidenciaID { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = "";

    [Required]
    public string Descripcion { get; set; } = "";

    [Required]
    public string Prioridad { get; set; } = "Media";

    [Required]
    public int TipoIncidenciaID { get; set; }

    [Required]
    public int EstatusID { get; set; }
    public int? AsignadoAID { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  SOLICITUDES
// ══════════════════════════════════════════════════════════════
public class SolicitudResumenDto
{
    public int SolicitudID { get; set; }
    public string Folio { get; set; } = "";
    public string Titulo { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public string NombreEstatus { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public string? ColorEstatus { get; set; }
    public string? FolioDashboard { get; set; }
    public string? NombreDashboard { get; set; }
    public string TipoSolicitud { get; set; } = "";
    public string SolicitadoPor { get; set; } = "";
    public string? AsignadoA { get; set; }
}

public class SolicitudDetalleDto
{
    public int SolicitudID { get; set; }
    public string Folio { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public int? DashboardID { get; set; }          // ← NUEVO
    public string? FolioDashboard { get; set; }    // ← NUEVO
    public string? NombreDashboard { get; set; }
    public string NombreEstatus { get; set; } = "";
    public string? ColorEstatus { get; set; }
    public string TipoSolicitud { get; set; } = "";
    public string Prioridad { get; set; } = "";
    public string ReportadoPor { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public string? AsignadoA { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public int TipoSolicitudID { get; set; }
    public int EstatusID { get; set; }
    public int? AsignadoAID { get; set; }
    public int SolicitadoPorID { get; set; }
    public string? AreaReportador { get; set; }
}

public class SolicitudCreateDto
{
    public int? DashboardID { get; set; }

    [Required(ErrorMessage = "Selecciona el tipo de solicitud.")]
    public int TipoSolicitudID { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(200)]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    public string Descripcion { get; set; } = "";

    public string? Justificacion { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    public string Prioridad { get; set; } = "Media";
}

public class SolicitudEditDto
{
    public int SolicitudID { get; set; }

    [Required]
    [MaxLength(200)]
    public string Titulo { get; set; } = "";

    [Required]
    public string Descripcion { get; set; } = "";

    public string? Justificacion { get; set; }

    [Required]
    public int TipoSolicitudID { get; set; }

    [Required]
    public int EstatusID { get; set; }
    public int? AsignadoAID { get; set; }
    public string Prioridad { get; set; } = "Media";
}

// ══════════════════════════════════════════════════════════════
//  COMENTARIOS
// ══════════════════════════════════════════════════════════════
public class ComentarioDto
{
    public int ComentarioID { get; set; }
    public string Autor { get; set; } = "";
    public string AutorCorreo { get; set; } = "";
    public bool EsRespuestaAdmin { get; set; }
    public string Mensaje { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public int? ComentarioPadreID { get; set; }
}

public class NuevoComentarioDto
{
    [Required] public string TipoRegistro { get; set; } = "";
    [Required] public int RegistroId { get; set; }
    [Required] public string Mensaje { get; set; } = "";
}

// ══════════════════════════════════════════════════════════════
//  AUDITORÍA
// ══════════════════════════════════════════════════════════════
public class AuditoriaDto
{
    public int AuditoriaID { get; set; }
    public string Usuario { get; set; } = "";
    public string Accion { get; set; } = "";
    public string? TablaAfectada { get; set; }
    public int? RegistroID { get; set; }
    public string? Detalle { get; set; }
    public DateTime FechaAccion { get; set; }
    public string? IPOrigen { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  NOTIFICACIONES
// ══════════════════════════════════════════════════════════════
public class NotificacionDto
{
    public int NotificacionID { get; set; }
    public int UsuarioID { get; set; }
    public string Titulo { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string? IconoClass { get; set; }
    public string? ColorClass { get; set; }
    public string? Url { get; set; }
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
}

// ══════════════════════════════════════════════════════════════
//  CAMBIAR ESTATUS (handlers de tickets)
// ══════════════════════════════════════════════════════════════
public class CambiarEstatusDto
{
    public int RegistroId { get; set; }
    public int EstatusId { get; set; }
    public int? AsignadoAId { get; set; }
    public string NuevoEstatusNombre { get; set; } = "";
}


// Agrega esta clase en Models/Dtos/Dtos.cs
// justo después de UnidadNegocioCreateDto

public class UnidadNegocioEditDto : UnidadNegocioCreateDto
{
    public int UnidadID { get; set; }
    // Activo ya viene del padre, no lo repitas
}

public class ImagenTicketDto
{
    public int ImagenID { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string RutaRelativa { get; set; } = "";
    public string TipoMime { get; set; } = "";
    public long TamanioBytes { get; set; }
    public DateTime FechaSubida { get; set; }
}
// ══════════════════════════════════════════════════════════════
//  AVISOS Y NOTICIAS — DTOs
//  Agregar en Models/Dtos/Dtos.cs
// ══════════════════════════════════════════════════════════════


// ── Aviso (lectura) ───────────────────────────────────────────
public class AvisoDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Contenido { get; set; } = "";
    public string Tipo { get; set; } = "Info";
    public DateTime Fecha { get; set; }
    public bool Activo { get; set; }
    public int CreadoPor { get; set; }
    public string CreadoPorNombre { get; set; } = "";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public int TotalComentarios { get; set; }
}

// ── Aviso (crear) ─────────────────────────────────────────────
public class AvisoCreateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(150)]
    public string Titulo { get; set; } = "";

    [Required(ErrorMessage = "El contenido es obligatorio.")]
    [MaxLength(1000)]
    public string Contenido { get; set; } = "";

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; } = "Info";

    public DateTime Fecha { get; set; } = DateTime.Today;
}

// ── Aviso (editar) ────────────────────────────────────────────
public class AvisoEditDto : AvisoCreateDto
{
    public int Id { get; set; }
    public bool Activo { get; set; }
}

// ── Comentario de Aviso (lectura) ─────────────────────────────
public class AvisoComentarioDto
{
    public int ComentarioID { get; set; }
    public int AvisoID { get; set; }
    public int UsuarioID { get; set; }
    public string AutorNombre { get; set; } = "";
    public string RolNombre { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public DateTime FechaComentario { get; set; }
}

// ── Comentario de Aviso (crear) ───────────────────────────────
public class AvisoComentarioCreateDto
{
    [Required]
    public int AvisoID { get; set; }

    [Required(ErrorMessage = "El mensaje no puede estar vacío.")]
    [MaxLength(1000)]
    public string Mensaje { get; set; } = "";
}

// ══════════════════════════════════════════════════════════════
//  AGREGAR en Models/Dtos/Dtos.cs
// ══════════════════════════════════════════════════════════════

public class AvisoReaccionDto
{
    public string Tipo { get; set; } = "";
    public int Total { get; set; }
    public int YoReaccione { get; set; }  // ← int, no bool
}