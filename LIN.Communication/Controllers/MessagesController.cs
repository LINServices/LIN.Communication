using LIN.Communication.Services.Models;

namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MessagesController : ControllerBase
{


    /// <summary>
    /// Obtiene la lista de mensajes asociados a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación</param>
    /// <param name="lastID">A partir del mensaje con Id</param>
    /// <param name="token">Token de acceso</param>
    [HttpGet("{id:int}/messages")]
    [LocalToken]
    public async Task<HttpReadAllResponse<MessageModel>> ReadAll([FromRoute] int id, [FromHeader] int lastID, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();


        // Valida el acceso Iam.
        var iam = await Services.Iam.Conversation.Validate(tokenInfo.ProfileId, id);

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
