using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace LIN.Communication.Hangfire.Services;

public class IdentityAuthorization : IDashboardAsyncAuthorizationFilter
{

    /// <summary>
    /// Autorización personalizada para Hangfire Dashboard.
    /// </summary>
    /// <param name="context">Contexto.</param>
    public async Task<bool> AuthorizeAsync(DashboardContext context)
    {
        // Obtener token JWT desde cookies.
        var httpContext = context.GetHttpContext();
        var token = httpContext.Request.Cookies["HangfireAuthToken"];

        // Si el token existe en la cookie, intentar validarlo.
        if (!string.IsNullOrEmpty(token))
        {
            var c = JwtService.Validate(token);
            if (!c)
                httpContext.Response.Cookies.Delete("HangfireAuthToken");
            return c;
        }

        // Si no hay token, mostrar formulario de autenticación.
        if (httpContext.Request.Method == "POST" && httpContext.Request.Form.ContainsKey("username") && httpContext.Request.Form.ContainsKey("password"))
        {
            var username = httpContext.Request.Form["username"].ToString() ?? "";
            var password = httpContext.Request.Form["password"].ToString() ?? "";

            string policy = Http.Services.Configuration.GetConfiguration("policy:current");

            // Validar en LIN Auth.
            var result = await Access.Auth.Controllers.Authentication.OnPolicy(username, password, policy ?? string.Empty);

            // Respuesta.
            if (result.Response != Types.Responses.Responses.Success)
            {
                httpContext.Response.StatusCode = 401;
                httpContext.Response.ContentType = "text/html";
                await httpContext.Response.WriteAsync(FileCache.ReadContent("wwwroot/pages/error.html").Replace("@MESSAGE", "Credenciales incorrectas"));
                return false;
            }

            // Generar el token JWT.
            var jwtToken = JwtService.Generate(username);

            // Almacenar el token JWT en las cookies.
            httpContext.Response.Cookies.Append("HangfireAuthToken", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Asegurarse de usar HTTPS en producción.
                Expires = DateTime.UtcNow.AddMinutes(30) // Token válido por 30 minutos.
            });

            return true;

        }

        // Si no hay token y no es un intento de autenticación, mostrar el formulario.
        httpContext.Response.StatusCode = 401;
        httpContext.Response.ContentType = "text/html";
        await httpContext.Response.WriteAsync(FileCache.ReadContent("wwwroot/pages/login.html"));
        return false;
    }

}
