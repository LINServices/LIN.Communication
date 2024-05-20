using LIN.Communication.Hubs;
using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Models;

namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MessagesController(IIamService Iam, IHubContext<ChatHub> hub) : ControllerBase
{


    /// <summary>
    /// Obtiene la lista de mensajes asociados a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación</param>
    /// <param name="lastID">A partir del mensaje con Id</param>
    /// <param name="token">Token de acceso</param>
    [HttpGet("{id:int}/messages")]
    [LocalToken]
    public async Task<HttpReadAllResponse<MessageModel>> ReadAll([FromRoute] int id, [FromHeader] int lastID, [FromHeader] string token)
    {

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Valida el acceso Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        // Valida el acceso Iam.
        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario.
        var result = await Data.Messages.ReadAll(id, lastID);

        // Retorna el resultado.
        return result ?? new();

    }




    /// <summary>
    /// Enviar un mensaje.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="message">Contenido del mensaje.</param>
    /// <param name="guid">Guid único</param>
    [HttpPost("{id:int}/messages")]
    [LocalToken]
    public async Task<HttpCreateResponse> Post([FromHeader] string token, [FromRoute] int id, [FromBody] string message, [FromQuery] string guid)
    {

        // Validar contenido.
        if (string.IsNullOrWhiteSpace(message))
            return new()
            {
                Message = "Message content can´t be empty",
                Response = Responses.InvalidParam,
            };

        // Información del token.
        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        // Valida el acceso Iam.
        var iam = await Iam.Validate(tokenInfo.ProfileId, id);

        // Valida el acceso Iam.
        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };


        // Hora actual.
        var time = DateTime.Now;

        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message,
            Remitente = new()
            {
                AccountID = tokenInfo.AccountId,
                Alias = tokenInfo.Alias,
                ID = tokenInfo.ProfileId
            },
            Time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0),
            Guid = guid,
            Conversacion = new()
            {
                ID = id
            }
        };

        // Envía el mensaje en tiempo real.
        await hub.Clients.Group(id.ToString()).SendAsync($"sendMessage", messageModel);

        // Crea el mensaje en la BD
        await Data.Messages.Create(messageModel);

        // Retorna el resultado.
        return new()
        {
            Response = Responses.Success
        };

    }

}