namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MessagesController : ControllerBase
{


    /// <summary>
    /// Obtiene la lista de mensajes asociados a una conversación.
    /// </summary>
    /// <param name="conversation">ID de la conversación</param>
    [HttpGet("{id}/messages")]
    public async Task<HttpReadAllResponse<MessageModel>> ReadAll([FromRoute] int conversation)
    {
        // Obtiene el usuario
        var result = await Data.Messages.ReadAll(conversation);

        // Retorna el resultado
        return result ?? new();

    }


}
