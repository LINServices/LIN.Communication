namespace LIN.Communication.Controllers;


[Route("conversations")]
public class ConversationController : ControllerBase
{



    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ConversationModel modelo)
    {

        // Obtiene el resultado
        var response = await Data.Conversations.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }



    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromHeader] string token)
    {

        // Comprobaciones
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.Unauthorized);

        // Obtiene el usuario
        var result = await Data.Conversations.ReadAll(profileID);


        var onHub = Hubs.ChatHub.Profiles.Values.Where(T => T.Profile.ID == profileID).FirstOrDefault();

        if (onHub != null)
        {
            onHub.Conversations = new();
            foreach (var c in result.Models)
                onHub.Conversations.Add(c.Conversation.Name);
        }

        // Retorna el resultado
        return result ?? new();

    }



    [HttpGet("read/one")]
    public async Task<HttpReadOneResponse<ConversationModel>> ReadOne([FromQuery] int id)
    {

        // Obtiene el usuario
        var result = await Data.Conversations.ReadOne(id);

        // Retorna el resultado
        return result ?? new();

    }



}