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


        // Validar modelo.
        if (modelo == null || string.IsNullOrWhiteSpace(modelo.Name))
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
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromHeader] string token, [FromHeader] string tokenAuth)
    {

        // Información del token.
        var (isValid, profileID, accountId, _) = Jwt.Validate(token);

        // Si el token es invalido.
        if (!isValid)
            return new(Responses.Unauthorized);

        // Obtiene el usuario.
        var result = await Data.Conversations.ReadAll(profileID);

        // Cuentas.
        List<int> accounts = [];

        foreach (var account in result.Models)
            accounts.AddRange(account.Conversation.Members.Select(t => t.Profile.AccountID));

        var x = await LIN.Access.Auth.Controllers.Account.Read(accounts, tokenAuth);


        // Sesión en memoria.
        var onHub = Mems.Sessions[profileID];
        if (onHub != null)
        {
            onHub.Conversations = [];
            foreach (var c in result.Models)
                onHub.Conversations.Add((c.Conversation.ID, c.Conversation.Name));
        }

        // Retorna el resultado
        return new ReadAllResponse<MemberChatModel>
        {
            Models = result.Models,
            AlternativeObject = x.Models,
            Response = result.Response,
        };

    }



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("read/one")]
    public async Task<HttpReadOneResponse<MemberChatModel>> ReadOne([FromQuery] int id, [FromHeader] string token, [FromHeader] string tokenAuth)
    {

        // Información del token.
        var (isValid, profileId, _, _) = Jwt.Validate(token);

        // Valida el token.
        if (!isValid)
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
        var result = await Data.Conversations.ReadOne(id, profileId);



        // Cuentas.
        List<int> accounts = result.Model.Conversation?.Members?.Select(t => t.Profile.AccountID).ToList() ?? [];


        var x = await LIN.Access.Auth.Controllers.Account.Read(accounts, tokenAuth);


        return new ReadOneResponse<MemberChatModel>()
        {
            AlternativeObject = x.Models,
            Model = result.Model,
            Response = result.Response,

        };

    }



}