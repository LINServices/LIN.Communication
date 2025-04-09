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

        // Obtener la información del token de acceso.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validar modelo.
        if (modelo is null || string.IsNullOrWhiteSpace(modelo.Name))
            return new()
            {
                Message = "El modelo es invalido.",
                Response = Responses.InvalidParam
            };

        // Organizar el modelo.
        modelo.Id = 0;
        modelo.Mensajes = [];
        modelo.Members ??= [];

        // Organizar la información de los integrantes..
        List<MemberChatModel> members = [];
        foreach (var member in modelo.Members)
        {
            member.Id = 0;
            if (member.Profile.Id == tokenInfo.ProfileId)
                continue;

            members.Add(member);
        }

        // Generar el integrante administrador.
        members.Add(new()
        {
            Profile = new()
            {
                Id = tokenInfo.ProfileId
            },
            Rol = MemberRoles.Admin
        });

        modelo.Members = members;

        // Crear en el reposotorio.
        var response = await conversationData.Create(modelo);

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
                onHub.Conversations.Add((c.Conversation.Id, c.Conversation.Name));
        }

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

        // Validación acceso con Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtener el modelo de la conversación.
        var result = await conversationData.Read(id, tokenInfo.ProfileId);

        // Obtener los ids de las cuentas de LIN Identity.
        List<int> accountIds = result.Model.Conversation?.Members?.Select(t => t.Profile.IdentityId).ToList() ?? [];

        // Obtener cuentas en el servicio de identidad.
        var accounts = await LIN.Access.Auth.Controllers.Account.ReadByIdentity(accountIds, tokenAuth);

        return new ReadOneResponse<MemberChatModel>()
        {
            AlternativeObject = accounts.Models,
            Model = result.Model,
            Response = result.Response
        };

    }


    /// <summary>
    /// Actualizar el nombre de una conversación.
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

        if (iam != IamLevels.Privileged)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Enviar la actualización al repositorio.
        var result = await conversationData.UpdateName(id, newName);

        return result;
    }

}