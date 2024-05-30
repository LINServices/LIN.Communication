namespace LIN.Communication.Services.Models;

public class JwtModel
{

    /// <summary>
    /// El token esta autenticado.
    /// </summary>
    public bool IsAuthenticated { get; set; }


    /// <summary>
    /// Alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;


    /// <summary>
    /// Id de la cuenta.
    /// </summary>
    public int IdentityId { get; set; }


    /// <summary>
    /// Id del perfil.
    /// </summary>
    public int ProfileId { get; set; }


}