using Microsoft.AspNetCore.Mvc.Filters;

namespace LIN.Communication.Services;

public class LocalTokenAttribute : ActionFilterAttribute
{

    /// <summary>
    /// Filtro del token.
    /// </summary>
    /// <param name="context">Contexto HTTP.</param>
    /// <param name="next">Siguiente.</param>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        // Contexto HTTP.
        var httpContext = context.HttpContext;

        // Obtiene el valor.
        bool can = httpContext.Request.Headers.TryGetValue("token", out Microsoft.Extensions.Primitives.StringValues value);

        // Información del token.
        var tokenInfo = Jwt.Validate(value.ToString());

        // Error de autenticación.
        if (!can || !tokenInfo.IsAuthenticated)
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsJsonAsync(new ResponseBase()
            {
                Message = "Token invalido.",
                Response = Responses.Unauthorized
            });
            return;
        }

        // Agrega la información del token.
        context.HttpContext.Items.Add(value.ToString(), tokenInfo);
        await base.OnActionExecutionAsync(context, next);

    }

}