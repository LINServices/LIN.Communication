using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Models;

namespace LIN.Communication.Controllers;


[Route("conversations")]
public class ConversationController(IIamService Iam) : ControllerBase
{


    /// <summary>
    /// Crear nueva conversación.
    /// </summary>
    /// <param name="modelo">Modelo.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPost]
    [LocalToken]
    public async Task<HttpCreateResponse> Create([FromBody] ConversationModel modelo, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

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
        var response = await Data.Conversations.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("all")]
    [LocalToken]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromHeader] string token, [FromHeader] string tokenAuth)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Obtiene el usuario.
        var result = await Data.Conversations.ReadAll(tokenInfo.ProfileId);

        // Cuentas.
        List<int> accounts = [];

        foreach (var account in result.Models)
            accounts.AddRange(account.Conversation.Members.Select(t => t.Profile.AccountID));

        var x = await LIN.Access.Auth.Controllers.Account.Read(accounts, tokenAuth);


        // Sesión en memoria.
        var onHub = Mems.Sessions[tokenInfo.ProfileId];
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
    [HttpGet]
    [LocalToken]
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
        var result = await Data.Conversations.Read(id, tokenInfo.ProfileId);



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



    /// <summary>
    /// Actualizar el nombre de un grupo.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="newName">Nuevo nombre.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpPatch("name")]
    [LocalToken]
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
        var result = await Data.Conversations.UpdateName(id, newName);

        return new()
        {
            Response = result.Response
        };

    }



}