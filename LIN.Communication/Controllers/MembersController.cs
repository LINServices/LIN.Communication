namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MembersController : ControllerBase
{


    /// <summary>
    /// Obtiene los miembros de una conversación
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    [HttpGet("{id}/members")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromRoute] int id)
    {

        // Obtiene el usuario
        var result = await Data.Conversations.ReadMembers(id);

        // Retorna el resultado
        return result ?? new();

    }


}
