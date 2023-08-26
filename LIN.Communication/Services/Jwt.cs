using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LIN.Communication.Services;


public class Jwt
{


    /// <summary>
    /// Llave del token
    /// </summary>
    private static string JwtKey { get; set; } = string.Empty;



    /// <summary>
    /// Inicia el servicio Jwt
    /// </summary>
    public static void Open()
    {
        JwtKey = Configuration.GetConfiguration("jwt:key");
    }




    /// <summary>
    /// Genera un JSON Web Token
    /// </summary>
    /// <param name="user">Modelo de usuario</param>
    internal static string Generate(ProfileModel user)
    {

        // Configuración
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtKey));

        // Credenciales
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        // Reclamaciones
        var claims = new[]
        {
            new Claim(ClaimTypes.PrimarySid, user.ID.ToString()),
            new Claim(ClaimTypes.UserData, user.AccountID.ToString()),
            new Claim(ClaimTypes.Name, user.Alias)
        };

        // Expiración del token
        var expiración = DateTime.Now.AddHours(5);

        // Token
        var token = new JwtSecurityToken(null, null, claims, null, expiración, credentials);

        // Genera el token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }



    /// <summary>
    /// Valida un JSON Web token
    /// </summary>
    /// <param name="token">Token a validar</param>
    internal static (bool isValid, int profileID, int userID, string alias) Validate(string token)
    {
        try
        {

            // Configurar la clave secreta
            var key = System.Text. Encoding.ASCII.GetBytes(JwtKey);

            // Validar el token
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
            };

            try
            {

                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;


                // Si el token es válido, puedes acceder a los claims (datos) del usuario
                _ = int.TryParse(jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.PrimarySid)?.Value, out int id);

                // 
                _ = int.TryParse(jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.UserData)?.Value, out int account);


                string name = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value ?? "";

                // Devuelve una respuesta exitosa
                return (true, id, account, name);
            }
            catch
            {

            }


        }
        catch { }

        return (false, 0, 0, "");

    }


}