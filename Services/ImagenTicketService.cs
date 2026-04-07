using CentralDashboards.Data;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CentralDashboards.Services;

// ============================================================
// Interfaz
// ============================================================
public interface IImagenTicketService
{
    Task<List<ImagenTicketDto>> GuardarImagenesAsync(
        string tipoTicket, int ticketId, int subidoPorId,
        IList<IFormFile> archivos);

    Task<List<ImagenTicketDto>> ObtenerPorTicketAsync(string tipoTicket, int ticketId);

    Task EliminarAsync(int imagenId);
}

// ============================================================
// Implementación
// ============================================================
public class ImagenTicketService : IImagenTicketService
{
    private readonly CentralDashboardsContext _db;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> _mimePermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
    };

    private const long MaxBytes = 5 * 1024 * 1024;

    public ImagenTicketService(CentralDashboardsContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<List<ImagenTicketDto>> GuardarImagenesAsync(
        string tipoTicket, int ticketId, int subidoPorId,
        IList<IFormFile> archivos)
    {
        var resultado = new List<ImagenTicketDto>();
        if (archivos == null || archivos.Count == 0) return resultado;

        var carpetaRelativa = Path.Combine("uploads", "tickets",
                                           tipoTicket.ToLower(), ticketId.ToString());
        var carpetaFisica = Path.Combine(_env.WebRootPath, carpetaRelativa);
        Directory.CreateDirectory(carpetaFisica);

        foreach (var file in archivos)
        {
            if (file.Length == 0) continue;
            if (file.Length > MaxBytes) continue;
            if (!_mimePermitidos.Contains(file.ContentType)) continue;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var nombreUnico = $"{Guid.NewGuid():N}{extension}";
            var rutaFisica = Path.Combine(carpetaFisica, nombreUnico);
            var rutaRelativa = "/" + Path.Combine(carpetaRelativa, nombreUnico)
                                        .Replace('\\', '/');

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
                await file.CopyToAsync(stream);

            var entidad = new TicketImagenEntity
            {
                TipoTicket = tipoTicket,
                TicketID = ticketId,
                NombreArchivo = Path.GetFileName(file.FileName),
                RutaRelativa = rutaRelativa,
                TipoMime = file.ContentType,
                TamanioBytes = file.Length,
                SubidoPorID = subidoPorId,
                FechaSubida = DateTime.Now
            };
            _db.TicketImagenes.Add(entidad);
            await _db.SaveChangesAsync();

            resultado.Add(new ImagenTicketDto
            {
                ImagenID = entidad.ImagenID,
                NombreArchivo = entidad.NombreArchivo,
                RutaRelativa = entidad.RutaRelativa,
                TipoMime = entidad.TipoMime,
                TamanioBytes = entidad.TamanioBytes,
                FechaSubida = entidad.FechaSubida
            });
        }
        return resultado;
    }

    public async Task<List<ImagenTicketDto>> ObtenerPorTicketAsync(string tipoTicket, int ticketId)
    {
        return await _db.TicketImagenes
            .Where(i => i.TipoTicket == tipoTicket && i.TicketID == ticketId)
            .OrderBy(i => i.FechaSubida)
            .Select(i => new ImagenTicketDto
            {
                ImagenID = i.ImagenID,
                NombreArchivo = i.NombreArchivo,
                RutaRelativa = i.RutaRelativa,
                TipoMime = i.TipoMime,
                TamanioBytes = i.TamanioBytes,
                FechaSubida = i.FechaSubida
            })
            .ToListAsync();
    }

    public async Task EliminarAsync(int imagenId)
    {
        var entidad = await _db.TicketImagenes.FindAsync(imagenId);
        if (entidad == null) return;

        var rutaFisica = Path.Combine(_env.WebRootPath,
                             entidad.RutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(rutaFisica))
            System.IO.File.Delete(rutaFisica);

        _db.TicketImagenes.Remove(entidad);
        await _db.SaveChangesAsync();
    }
}