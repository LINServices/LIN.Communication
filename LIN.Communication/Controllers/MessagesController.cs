using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Interfaces;
using LIN.Communication.Services.Models;

namespace LIN.Communication.Controllers;

[Route("conversations")]
public class MessagesController(IMessageSender messageSender, IIamService Iam, Persistence.Data.Messages messagesData) : ControllerBase
{

    /// <summary>
    /// Obtiene la lista de mensajes asociados a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación</param>
    /// <param name="lastID">A partir del mensaje con Id</param>
    /// <param name="token">Token de acceso</param>
    [HttpGet("{id:int}/messages")]
    [LocalToken]
    [RateLimit(requestLimit: 5, timeWindowSeconds: 30, blockDurationSeconds: 60)]
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
        var result = await messagesData.ReadAll(id, lastID);

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
    [RateLimit(requestLimit: 20, timeWindowSeconds: 10, blockDurationSeconds: 100)]
    public async Task<HttpCreateResponse> Post([FromHeader] string token, [FromRoute] int id, [FromBody] string message, [FromQuery] string guid, [FromQuery] DateTime? sendAt)
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

        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message,
            Remitente = new()
            {
                IdentityId = tokenInfo.IdentityId,
                Alias = tokenInfo.Alias,
                Id = tokenInfo.ProfileId
            },
            Time = DateTime.Now,
            Guid = guid,
            Conversacion = new()
            {
                Id = id
            }
        };

        // Enviar mensaje.
        var response = await messageSender.Send(messageModel, guid, tokenInfo, sendAt);

        // Retorna el resultado.
        return new()
        {
            Response = response.Response,
            Errors = response.Errors,
            Message = response.Message
        };

    }

}