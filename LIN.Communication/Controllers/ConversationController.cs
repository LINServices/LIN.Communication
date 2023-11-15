using System.Diagnostics.CodeAnalysis;

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
        if (isValid)
            return new()
            {
                Message = "El token es invalido.",
                Response = Responses.Unauthorized
            };

        // Organizar el modelo.
        modelo.ID = 0;
        modelo.Mensajes = new();
        modelo.Members ??= new();

        // Integrantes.
        List<MemberChatModel> members = new();
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
            Rol = Types.Communication.Enumerations.MemberRoles.Admin
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
            onHub.Conversations = new();
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
    [Experimental("Este método no tiene las medidas de seguridad requerida.")]
    public async Task<HttpReadOneResponse<ConversationModel>> ReadOne([FromQuery] int id, [FromHeader] string token)
    {

        // Obtiene el usuario
        var result = await Data.Conversations.ReadOne(id);

        // Retorna el resultado
        return result ?? new();

    }



}