// ============================================================
// Models/Entities/TicketImagenEntity.cs
// ============================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CentralDashboards.Models.Entities;

[Table("TicketImagenes")]
public class TicketImagenEntity
{
    [Key]
    public int ImagenID { get; set; }

    [Required, MaxLength(20)]
    public string TipoTicket { get; set; } = "";   // "Incidencia" | "Solicitud"

    public int TicketID { get; set; }

    [Required, MaxLength(260)]
    public string NombreArchivo { get; set; } = "";

    [Required, MaxLength(500)]
    public string RutaRelativa { get; set; } = "";

    [Required, MaxLength(100)]
    public string TipoMime { get; set; } = "";

    public long TamanioBytes { get; set; }

    public int SubidoPorID { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
