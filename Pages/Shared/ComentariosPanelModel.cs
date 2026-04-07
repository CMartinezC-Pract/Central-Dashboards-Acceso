using CentralDashboards.Models.Dtos;

namespace CentralDashboards.Pages.Shared;

/// <summary>
/// Modelo que se pasa al partial _ComentariosPanel.cshtml
/// </summary>
public class ComentariosPanelModel
{
    public string              TipoRegistro { get; set; } = "";  // "Incidencia" | "Solicitud"
    public int                 RegistroId   { get; set; }
    public List<ComentarioDto> Comentarios  { get; set; } = new();
}
