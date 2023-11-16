namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MessagesController : ControllerBase
{


    /// <summary>
    /// Obtiene la lista de mensajes asociados a una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación</param>
    /// <param name="lastID">A partir del mensaje con ID</param>
    /// <param name="token">Token de acceso</param>
    [HttpGet("{id:int}/messages")]
    public async Task<HttpReadAllResponse<MessageModel>> ReadAll([FromRoute] int id, [FromHeader] int lastID, [FromHeader] string token)
    {

        // Obtiene la info del token.
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        // Token es invalido.
        if (!isValid)
            return new ReadAllResponse<MessageModel>()
            {
                Message = "El token es invalido.",
                Response = Responses.Unauthorized
            };

        // Valida el acceso Iam.
        var iam = await Services.Iam.Conversation.Validate(profileID, id);

        // Valida el acceso Iam.
        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario.
        var result = await Data.Messages.ReadAll(id, lastID);

        // Retorna el resultado.
        return result ?? new();

    }



}
