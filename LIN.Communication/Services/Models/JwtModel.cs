namespace LIN.Communication.Services.Models;

public class JwtModel
{

    /// <summary>
    /// Obtiene o establece si el token es valido.
    /// </summary>
    public bool IsAuthenticated { get; set; }


    /// <summary>
    /// Alias del perfil de la cuenta.
    /// </summary>
    public string Alias { get; set; } = string.Empty;


    /// <summary>
    /// Id de la identidad en LIN Identity.
    /// </summary>
    public int IdentityId { get; set; }


    /// <summary>
    /// Id del perfil (LIN Communication).
    /// </summary>
    public int ProfileId { get; set; }

}