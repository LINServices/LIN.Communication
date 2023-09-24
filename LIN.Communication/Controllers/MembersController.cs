namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MembersController : ControllerBase
{


    /// <summary>
    /// Un usuario esta online
    /// </summary>
    /// <param name="id">ID del usuario</param>
    [HttpGet("isOnline")]
    public HttpReadOneResponse<IsOnlineResult> ReadOnline([FromQuery] int id)
    {

        // Obtiene el perfil
        var profile = Hubs.ChatHub.Profiles.Where(T => T.Key == id).FirstOrDefault().Value;

        return new ReadOneResponse<IsOnlineResult>()
        {
            Response = Responses.Success,
            Model =  new()
            {
                ID = id,
                IsOnline = profile?.Devices.Any() ?? false,
                LastTime = profile?.LastTime ?? new(),
            } 
        };

    }



    /// <summary>
    /// Obtiene los miembros de una conversación
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
        var have = await Data.Conversations.HaveAccessFor(profileID, id);

        // Si no tiene acceso
        if (have.Response != Responses.Success)
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
    public async Task<HttpReadAllResponse<SessionModel<MemberChatModel>>> ReadAllInfo([FromRoute] int id, [FromHeader] string token)
    {

        // Obtiene el usuario
        var result = await Data.Conversations.ReadMembers(id);

        var x = result.Models.Select(T => T.Profile.AccountID).ToList();

        var resultAccounts = await LIN.Access.Auth.Controllers.Account.Read(x, token);


        var re = (from P in result.Models
                  join A in resultAccounts.Models
                  on P.Profile.AccountID equals A.ID
                  select new SessionModel<MemberChatModel>
                  {
                      Account = A,
                      Profile = new()
                      {
                          Rol = P.Rol,
                          Profile = new()
                          {
                              ID = P.Profile.ID,
                              Alias = P.Profile.Alias
                          }

                      }
                  }).ToList();


        // Retorna el resultado
        return new ReadAllResponse<SessionModel<MemberChatModel>>
        {
            Models = re,
            Response = Responses.Success
        };

    }


}