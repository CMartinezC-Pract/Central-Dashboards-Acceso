using CentralDashboards.Helpers;
using CentralDashboards.Models.Dtos;
using CentralDashboards.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CentralDashboards.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IDashboardService _dash;
    private readonly IUnidadNegocioService _unidades;
    private readonly IAvisoService _avisos;
    private readonly ISolicitudService _solicitudes;

    public IndexModel(
        IDashboardService dash,
        IUnidadNegocioService unidades,
        IAvisoService avisos,
        ISolicitudService solicitudes)
    {
        _dash = dash;
        _unidades = unidades;
        _avisos = avisos;
        _solicitudes = solicitudes;
    }

    [BindProperty(SupportsGet = true)] public int? UnidadId { get; set; }
    [BindProperty(SupportsGet = true)] public bool VerAreas { get; set; }

    public List<UnidadNegocioDto> Unidades { get; set; } = new();
    public List<DashboardDto> Dashboards { get; set; } = new();
    public List<DashboardDto> DashboardsRecientes { get; set; } = new();
    public List<AvisoDto> Avisos { get; set; } = new();
    public int TotalDashboards { get; set; }
    public HashSet<int> DashboardsYaSolicitados { get; set; } = new();

    public async Task OnGetAsync()
    {
        var uid = UserHelper.GetUsuarioId(User);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        Unidades = await _unidades.ObtenerTodasAsync();
        Console.WriteLine($"[PERF] Unidades: {sw.ElapsedMilliseconds}ms");
        sw.Restart();

        if (UnidadId.HasValue)
        {
            Dashboards = await _dash.ObtenerPorUnidadAsync(UnidadId.Value, uid);
            Console.WriteLine($"[PERF] Dashboards por unidad: {sw.ElapsedMilliseconds}ms");
        }
        else
        {
            Avisos = await _avisos.ObtenerTodosAsync(true);
            Console.WriteLine($"[PERF] Avisos: {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            var todos = await _dash.ObtenerTodosAsync(null, true, uid);
            Console.WriteLine($"[PERF] Dashboards todos: {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            TotalDashboards = todos.Count;
            DashboardsRecientes = todos.OrderByDescending(d => d.FechaPublicacion).Take(3).ToList();

            DashboardsYaSolicitados = await _solicitudes.ObtenerDashboardsYaSolicitadosAsync(uid);
            Console.WriteLine($"[PERF] Solicitudes: {sw.ElapsedMilliseconds}ms");
        }
    }
}