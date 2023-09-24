namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MembersController : ControllerBase
{



    /// <summary>
    /// Obtiene los miembros de una conversación
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
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
    [HttpGet("test")]
    public async Task<HttpReadAllResponse<string>> ReadOe([FromQuery] int id)
    {

        // Obtiene el perfil
        var profile = Hubs.ChatHub.Profiles.Where(T => T.Key == id).FirstOrDefault().Value;

        return new ReadAllResponse<string>()
        {
            Response = Responses.Success,
            Models = profile?.Devices ?? new(),
        };

    }




    /// <summary>
    /// Obtiene los miembros de una conversación
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    [HttpGet("{id}/members")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromRoute] int id)
    {

        // Obtiene el usuario
        var result = await Data.Conversations.ReadMembers(id);

        // Retorna el resultado
        return result ?? new();

    }




    /// <summary>
    /// Obtiene los miembros de una conversación
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    [HttpGet("{id}/members/info")]
    public async Task<HttpReadAllResponse<SessionModel<ProfileModel>>> ReadAllInfo([FromRoute] int id, [FromHeader] string token)
    {

        // Obtiene el usuario
        var result = await Data.Conversations.ReadMembers(id);

        var x = result.Models.Select(T => T.Profile.AccountID).ToList();

        var resultAccounts = await LIN.Access.Auth.Controllers.Account.Read(x, token);


        var re = (from P in result.Models
                  join A in resultAccounts.Models
                  on P.Profile.AccountID equals A.ID
                  select new SessionModel<ProfileModel>
                  {
                      Account = A,
                      Profile = P.Profile
                  }).ToList();


        // Retorna el resultado
        return new ReadAllResponse<SessionModel<ProfileModel>>
        {
            Models = re,
            Response = Responses.Success
        };

    }


}
