namespace LIN.Communication.Controllers;


[Route("conversations")]
public class ConversationController : ControllerBase
{


    /// <summary>
    /// Crea una nueva conversación.
    /// </summary>
    /// <param name="modelo">Modelo</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ConversationModel modelo)
    {

        // Obtiene el resultado
        var response = await Data.Conversations.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromHeader] string token)
    {

        // Comprobaciones
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.Unauthorized);

        // Obtiene el usuario
        var result = await Data.Conversations.ReadAll(profileID);

        // Retorna el resultado
        return result ?? new();

    }


}