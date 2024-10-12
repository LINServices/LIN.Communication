using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Models;

namespace LIN.Communication.Controllers;

[Route("conversations")]
[RateLimit(requestLimit: 10, timeWindowSeconds: 20, blockDurationSeconds: 300)]
public class MembersController(IIamService Iam, Persistence.Data.Conversations conversationData, Persistence.Data.Profiles profilesData, Persistence.Data.Members membersData) : ControllerBase
{

    /// <summary>
    /// Validación si un usuario esta online.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    [HttpGet("isOnline")]
    public async Task<HttpReadOneResponse<IsOnlineResult>> ReadOnline([FromQuery] int id)
    {

        // Obtiene el perfil
        var profile = Mems.Sessions[id];

        // Perfil no existe.
        if (profile is null)
            return new()
            {
                Message = "Sesión no encontrada",
                Response = Responses.NotRows
            };

        return new ReadOneResponse<IsOnlineResult>()
        {
            Response = Responses.Success,
            Model = new()
            {
                ID = id,
                IsOnline = profile?.Devices.Count != 0,
                LastTime = profile?.LastTime ?? (await profilesData.GetLastConnection(id)).Model,
            }
        };

    }


    /// <summary>
    /// Obtiene los miembros de una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("{id:int}/members")]
    [LocalToken]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromRoute] int id, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Busca el acceso
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        if (iam == IamLevels.NotAccess)
            return new ReadAllResponse<MemberChatModel>
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario
        var result = await membersData.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }


    /// <summary>
    /// Obtiene los miembros de una conversación con info del usuario.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("{id:int}/members/info")]
    [LocalToken]
    public async Task<HttpReadAllResponse<SessionModel<MemberChatModel>>> ReadAllInfo([FromRoute] int id, [FromHeader] string token, [FromHeader] string tokenAuth)
    {
        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validación Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        // Valida el acceso Iam.
        if (iam == Types.Enumerations.IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };


        // Obtiene los miembros.
        var members = await membersData.ReadAll(id);

        // Obtiene los Id de las cuentas.
        var accountsId = members.Models.Select(member => member.Profile.IdentityId).ToList();

        // Información de las cuentas.
        var accounts = await Access.Auth.Controllers.Account.ReadByIdentity(accountsId, tokenAuth);

        // Armar los modelos.
        var response = (from member in members.Models
                        join account in accounts.Models
                        on member.Profile.IdentityId equals account.IdentityId
                        select new SessionModel<MemberChatModel>
                        {
                            Account = account,
                            Profile = new()
                            {
                                Rol = member.Rol,
                                Profile = new()
                                {
                                    ID = member.Profile.ID,
                                    Alias = member.Profile.Alias,
                                    LastConnection = member.Profile.LastConnection,
                                }
                            }
                        }).ToList();

        // Retorna el resultado
        return new ReadAllResponse<SessionModel<MemberChatModel>>
        {
            Models = response,
            Response = Responses.Success
        };

    }


    /// <summary>
    /// Agrega un miembro a una conversación.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("{id:int}/members/add")]
    [LocalToken]
    public async Task<HttpResponseBase> AddTo([FromRoute] int id, [FromQuery] int profileId, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validación Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        // Valida el acceso Iam.
        if (iam != IamLevels.Privileged)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes permisos para modificar a esta conversación."
            };

        // Insertar el integrante.
        var response = await membersData.Create(id, profileId);

        return new()
        {
            Message = response.Response == Responses.Success ? "Correcto" : "No se pudo insertar el integrante.",
            Response = response.Response
        };
    }


    /// <summary>
    /// Eliminar un miembro a una conversación.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}/members")]
    [LocalToken]
    public async Task<HttpResponseBase> Delete([FromRoute] int id, [FromQuery] int profileId, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();


        // Validación Iam.
        if (tokenInfo.ProfileId != profileId)
        {
            var iam = await Iam.Validate(tokenInfo.ProfileId, id);

            // Valida el acceso Iam.
            if (iam != IamLevels.Privileged)
                return new()
                {
                    Response = Responses.Unauthorized,
                    Message = "No tienes permisos para modificar a esta conversación."
                };
        }

        // Insertar el integrante.
        var response = await membersData.Remove(id, profileId);

        return new()
        {
            Message = response.Response == Responses.Success ? "Correcto" : "No se pudo eliminar el integrante.",
            Response = response.Response
        };
    }


    /// <summary>
    /// Encuentra o crea una conversación personal.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="friendId">Id del otro usuario.</param>
    [HttpPost("find")]
    [LocalToken]
    public async Task<HttpCreateResponse> Find([FromHeader] string token, [FromHeader] int friendId)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Validar parámetros.
        if (friendId <= 0)
            return new()
            {
                Message = "El 'Id' es menor o igual a 0",
                Response = Responses.InvalidParam
            };

        // Consulta.
        var conversation = await conversationData.Find(friendId, tokenInfo.ProfileId);

        // Si ya existe.
        if (conversation.Model is not null)
        {
            return new CreateResponse()
            {
                Response = Responses.Success,
                LastID = conversation.Model.ID,
                Message = "Se encontró."
            };
        }

        // Crear el chat
        var response = await conversationData.Create(new()
        {
            ID = 0,
            Name = "Chat Personal",
            Type = ConversationsTypes.Personal,
            Visibility = ConversationVisibility.@public,
            Members = [
                 new MemberChatModel()
                 {
                     ID = 0,
                     Profile = new()
                     {
                         ID = tokenInfo.ProfileId
                     },
                     Rol = MemberRoles.Admin
                 },
                 new MemberChatModel()
                 {
                     ID = 0,
                     Profile = new()
                     {
                        ID = friendId
                     },
                     Rol = MemberRoles.Admin
                 }
            ]
        });

        // Retorna el resultado
        return response;

    }

}