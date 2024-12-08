using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LIN.Communication.Hangfire.Services;

public class JwtService
{

    /// <summary>
    /// Llave del token
    /// </summary>
    private static string JwtKey { get; set; } = string.Empty;


    /// <summary>
    /// Inicia el servicio JwtService
    /// </summary>
    public static void Open()
    {
        JwtKey = Http.Services.Configuration.GetConfiguration("jwt:key_hangfire");
    }


    /// <summary>
    /// Genera un JSON Web Token
    /// </summary>
    /// <param name="user">Modelo de usuario</param>
    public static string Generate(string user)
    {

        if (JwtKey == string.Empty)
            Open();

        // Configuración
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));

        // Credenciales
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        // Reclamaciones
        var claims = new[]
        {
            new Claim(ClaimTypes.PrimarySid, user)
        };

        // Expiración del token
        var expiración = DateTime.Now.AddMinutes(30);

        // Token
        var token = new JwtSecurityToken(null, null, claims, null, expiración, credentials);

        // Genera el token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    /// <summary>
    /// Valida un JSON Web token
    /// </summary>
    /// <param name="token">Token a validar</param>
    public static bool Validate(string token)
    {
        try
        {

            // Comprobación
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Configurar la clave secreta
            var key = Encoding.ASCII.GetBytes(JwtKey);

            // Validar el token
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true
            };

            try
            {

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Si el token es válido, puedes acceder a los claims (datos) del usuario
                var user = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                // 
                return true;
            }
            catch (SecurityTokenException)
            {
            }


        }
        catch { }

        return false;

    }

}