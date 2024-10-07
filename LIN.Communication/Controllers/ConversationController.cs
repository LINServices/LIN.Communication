using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Models;

namespace LIN.Communication.Controllers;

[LocalToken]
[Route("conversations")]
[RateLimit(requestLimit: 10, timeWindowSeconds: 30, blockDurationSeconds: 300)]
public class ConversationController(IIamService Iam, Persistence.Data.Conversations conversationData) : ControllerBase
{

    /// <summary>
    /// Crear nueva conversación.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost]
    public async Task<HttpCreateResponse> Create([FromBody] ConversationModel modelo, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validar modelo.
        if (modelo is null || string.IsNullOrWhiteSpace(modelo.Name))
            return new()
            {
                Message = "El modelo es invalido.",
                Response = Responses.InvalidParam
            };

        // Organizar el modelo.
        modelo.ID = 0;
        modelo.Mensajes = [];
        modelo.Members ??= [];

        // Integrantes.
        List<MemberChatModel> members = [];
        foreach (var member in modelo.Members)
        {
            member.ID = 0;
            if (member.Profile.ID == tokenInfo.ProfileId)
                continue;

            members.Add(member);
        }

        // Agrega al administrador.
        members.Add(new()
        {
            Profile = new()
            {
                ID = tokenInfo.ProfileId
            },
            Rol = MemberRoles.Admin
        });

        // Establecer los miembros.
        modelo.Members = members;

        // Obtiene el resultado
        var response = await conversationData.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }


    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("all")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Obtiene el usuario.
        var result = await conversationData.ReadAll(tokenInfo.ProfileId);

        // Sesión en memoria.
        var onHub = Mems.Sessions[tokenInfo.ProfileId];
        if (onHub is not null)
        {
            onHub.Conversations = [];
            foreach (var c in result.Models)
                onHub.Conversations.Add((c.Conversation.ID, c.Conversation.Name));
        }

        // Retorna el resultado
        return new ReadAllResponse<MemberChatModel>
        {
            Models = result.Models,
            Response = result.Response,
        };

    }


    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<MemberChatModel>> ReadOne([FromQuery] int id, [FromHeader] string token, [FromHeader] string tokenAuth)
    {
        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validación Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        // Valida el acceso Iam.
        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario
        var result = await conversationData.Read(id, tokenInfo.ProfileId);

        // Cuentas.
        List<int> accountIds = result.Model.Conversation?.Members?.Select(t => t.Profile.IdentityId).ToList() ?? [];

        // Obtener cuentas.
        var accounts = await LIN.Access.Auth.Controllers.Account.ReadByIdentity(accountIds, tokenAuth);

        return new ReadOneResponse<MemberChatModel>()
        {
            AlternativeObject = accounts.Models,
            Model = result.Model,
            Response = result.Response
        };

    }


    /// <summary>
    /// Actualizar el nombre de un grupo.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="newName">Nuevo nombre.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPatch("name")]
    public async Task<HttpResponseBase> UpdateName([FromQuery] int id, [FromQuery] string newName, [FromHeader] string token)
    {

        if (string.IsNullOrWhiteSpace(newName))
            return new()
            {
                Message = "El nombre no puede ser vació.",
                Response = Responses.InvalidParam
            };

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validación Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        // Valida el acceso Iam.
        if (iam != IamLevels.Privileged)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario
        var result = await conversationData.UpdateName(id, newName);

        return new()
        {
            Response = result.Response
        };

    }

}