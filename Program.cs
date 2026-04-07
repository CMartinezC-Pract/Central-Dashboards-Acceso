using CentralDashboards.Data;
using CentralDashboards.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

// CORRECCIÓN: se eliminó "using static CentralDashboards.Services.NotificacionService"
// IAvisoService ya no está anidada dentro de NotificacionService — vive en el namespace raíz.

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContextPool<CentralDashboardsContext>(options =>
    options.UseSqlServer(connectionString, sql =>
    {
        sql.CommandTimeout(30);
        sql.EnableRetryOnFailure(3);
    })
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
    poolSize: 10);

// ── Caché en memoria — dashboards y catálogos ─────────────────
builder.Services.AddMemoryCache();

// ── Compresión gzip — reduce ~60-70% el peso de cada respuesta HTML/JSON ──
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = ".CentralDashboards.Auth";
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("SoloAdmin", p => p.RequireRole("Administrador")));

// ── Registro de servicios ─────────────────────────────────────
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IIncidenciaService, IncidenciaService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IUnidadNegocioService, UnidadNegocioService>();
builder.Services.AddScoped<IImagenTicketService, ImagenTicketService>();
builder.Services.AddScoped<IAvisoService, AvisoService>();
builder.Services.AddHttpContextAccessor();

// ── Límite de uploads — 30 MB ─────────────────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 30 * 1024 * 1024;
});

builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// OPTIMIZACIÓN: gzip debe ir antes de cualquier middleware que genere respuestas
app.UseResponseCompression();

app.UseHttpsRedirection();

// OPTIMIZACIÓN: cache de 7 días para CSS, JS, imágenes — el browser no los vuelve a pedir
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append(
            "Cache-Control", "public, max-age=604800, immutable");
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Anti-caché para páginas autenticadas (se mantiene igual) ──
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";
    }
    await next();
});

app.MapRazorPages();
// Precalentar EF Core para evitar delay en primer request
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<CentralDashboards.Data.CentralDashboardsContext>();
    await db.Database.ExecuteSqlRawAsync("SELECT 1");
}

app.Run();
