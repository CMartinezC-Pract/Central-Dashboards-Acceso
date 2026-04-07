using System.Security.Claims;

namespace CentralDashboards.Helpers;

public static class UserHelper
{
    public static int GetUsuarioId(ClaimsPrincipal user)
        => int.TryParse(user.FindFirstValue("UsuarioID"), out var id) ? id : 0;

    public static string GetNombre(ClaimsPrincipal user)
        => user.FindFirstValue("Nombre") ?? "Usuario";

    public static string GetRol(ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role) ?? "";

    public static bool EsAdmin(ClaimsPrincipal user)
        => user.IsInRole("Administrador");
}
