using System.Diagnostics.CodeAnalysis;

namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MembersController : ControllerBase
{


    /// <summary>
    /// Validación si un usuario esta online.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    /// <returns></returns>
    [HttpGet("isOnline")]
    public async Task<HttpReadOneResponse<IsOnlineResult>> ReadOnline([FromQuery] int id)
    {

        // Obtiene el perfil
        var profile = Mems.Sessions[id];

        return new ReadOneResponse<IsOnlineResult>()
        {
            Response = Responses.Success,
            Model = new()
            {
                ID = id,
                IsOnline = profile?.Devices.Count != 0,
                LastTime = profile?.LastTime ?? (await Data.Profiles.GetLastConnection(id)).Model,
            }
        };

    }



    /// <summary>
    /// Obtiene los miembros de una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("{id}/members")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromRoute] int id, [FromHeader] string token)
    {

        // Obtiene la info del token
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        // Token es invalido
        if (!isValid)
            return new ReadAllResponse<MemberChatModel>()
            {
                Message = "El token es invalido.",
                Response = Responses.Unauthorized
            };

        // Busca el acceso
        var iam = await Services.Iam.Conversation.Validate(profileID, id);

        if (iam == Types.Enumerations.IamLevels.NotAccess)
            return new ReadAllResponse<MemberChatModel>
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario
        var result = await Data.Conversations.ReadMembers(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene los miembros de una conversación con info del usuario.
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("{id}/members/info")]
    public async Task<HttpReadAllResponse<SessionModel<MemberChatModel>>> ReadAllInfo([FromRoute] int id, [FromHeader] string token, [FromHeader] string tokenAuth)
    {

        // Información del token.
        var (isValid, profile, _, _) = Jwt.Validate(token);

        // Si el token es invalido.
        if (!isValid)
            return new()
            {
                Message = "Token invalido.",
                Response = Responses.Unauthorized
            };

        // Validación Iam.
        var iam = await Services.Iam.Conversation.Validate(profile, id);

        // Valida el acceso Iam.
        if (iam == Types.Enumerations.IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };


        // Obtiene los miembros.
        var members = await Data.Conversations.ReadMembers(id);

        // Obtiene los Id de las cuentas.
        var accountsId = members.Models.Select(member => member.Profile.AccountID).ToList();

        // Información de las cuentas.
        var accounts = await Access.Auth.Controllers.Account.Read(accountsId, tokenAuth);

        // Armar los modelos.
        var response = (from member in members.Models
                  join account in accounts.Models
                  on member.Profile.AccountID equals account.ID
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
    [HttpGet("{id}/members/add")]
    [Experimental("Este método no esta completo y faltan los limites de seguridad.")]
    public async Task<HttpResponseBase> AddTo([FromRoute] int id, [FromHeader] string token)
    {

        var account = await LIN.Access.Auth.Controllers.Account.Read(id, token);

        if (account.Response != Responses.Success)
        {
            return new ResponseBase()
            {
                Message = "Cuenta invalida",
                Response = account.Response
            };
        }


        var getProfile = await Data.Profiles.ReadByAccount(account.Model.ID);

        if (getProfile.Response == Responses.NotExistProfile)
        {

            // Crear perfil
            var res = await Data.Profiles.Create(new()
            {
                Account = account.Model,
                Profile = new()
                {
                    AccountID = account.Model.ID,
                    Alias = account.Model.Nombre
                }
            });

            if (res.Response != Responses.Success)
            {
                return new ReadOneResponse<AuthModel<ProfileModel>>
                {
                    Response = Responses.UnavailableService,
                    Message = "Un error grave ocurrió"
                };
            }

            getProfile.Model = res.Model;

        }


        return await Data.Conversations.InsertMember(id, getProfile.Model.ID);



    }


}