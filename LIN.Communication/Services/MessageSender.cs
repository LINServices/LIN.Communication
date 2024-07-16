using LIN.Communication.Hubs;
using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Models;

namespace LIN.Communication.Services;


public class MessageSender(IIamService IamService, IHubContext<ChatHub> hub) : Interfaces.IMessageSender
{


    /// <summary>
    /// Enviar mensaje.
    /// </summary>
    /// <param name="message">Modelo del mensaje.</param>
    /// <param name="guid">New guid.</param>
    /// <param name="sender">Remitente.</param>
    public async Task<ResponseBase> Send(MessageModel message, string guid, JwtModel sender)
    {
        return await Send(message, guid, new ProfileModel()
        {
            ID = sender.ProfileId,
            Alias = sender.Alias,
            IdentityId = sender.IdentityId,
        });
    }



    /// <summary>
    /// Enviar mensaje
    /// </summary>
    /// <param name="message">Modelo.</param>
    /// <param name="guid">Guid.</param>
    /// <param name="sender">Autenticación.</param>
    public async Task<ResponseBase> Send(MessageModel message, string guid, ProfileModel sender)
    {

        // Validar modelo.
        if (message is null || string.IsNullOrWhiteSpace(message.Contenido))
            return new()
            {
                Message = "Message content can´t be empty",
                Response = Responses.InvalidParam,
            };

        // Iam.
        IamLevels iam = await IamService.Validate(sender.ID, message.Conversacion.ID);

        // Not access.
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
            Contenido = message.Contenido,
            Remitente = new()
            {
                IdentityId = sender.IdentityId,
                Alias = sender.Alias,
                ID = sender.ID
            },
            Time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0),
            Guid = guid,
            Conversacion = new()
            {
                ID = message.Conversacion.ID
            }
        };

        // Envía el mensaje en tiempo real.
        await hub.Clients.Group(message.Conversacion.ID.ToString()).SendAsync($"sendMessage", messageModel);

        // Crea el mensaje en la BD
        await Data.Messages.Create(messageModel);

        // Retorna el resultado.
        return new()
        {
            Response = Responses.Success
        };

    }


}