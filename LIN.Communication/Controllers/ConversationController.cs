namespace LIN.Communication.Controllers;


[Route("conversations")]
public class ConversationController : ControllerBase
{


    /// <summary>
    /// Crear nueva conversación.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ConversationModel modelo, [FromHeader] string token)
    {

        // Información del token.
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        // Valida el token.
        if (!isValid)
            return new()
            {
                Message = "El token es invalido.",
                Response = Responses.Unauthorized
            };

        // Organizar el modelo.
        modelo.ID = 0;
        modelo.Mensajes = [];
        modelo.Members ??= [];

        // Integrantes.
        List<MemberChatModel> members = [];
        foreach(var member in modelo.Members)
        {
            member.ID = 0;
            if (member.Profile.ID == profileID)
                continue;

            members.Add(member);
        }

        // Agrega al administrador.
        members.Add(new()
        {
            Profile = new()
            {
                ID = profileID
            },
            Rol = MemberRoles.Admin
        });

        // Establecer los miembros.
        modelo.Members = members;

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

        // Información del token.
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        // Si el token es invalido.
        if (!isValid)
            return new(Responses.Unauthorized);

        // Obtiene el usuario.
        var result = await Data.Conversations.ReadAll(profileID);

        // Sesión en memoria.
        var onHub = Mems.Sessions[profileID];
        if (onHub != null)
        {
            onHub.Conversations = [];
            foreach (var c in result.Models)
                onHub.Conversations.Add(c.Conversation.Name);
        }

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read/one")]
    public async Task<HttpReadOneResponse<ConversationModel>> ReadOne([FromQuery] int id, [FromHeader] string token)
    {

        // Información del token.
        var (isValid, profileId, _, _) = Jwt.Validate(token);

        // Valida el token.
        if (isValid)
            return new()
            {
                Message = "El token es invalido.",
                Response = Responses.Unauthorized
            };

        // Validación Iam.
        var iam = await Services.Iam.Conversation.Validate(profileId, id);

        // Valida el acceso Iam.
        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario
        var result = await Data.Conversations.ReadOne(id);

        // Retorna el resultado
        return result ?? new();

    }



}