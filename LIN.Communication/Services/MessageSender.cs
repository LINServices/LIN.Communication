using Hangfire;
using LIN.Communication.Hubs;

namespace LIN.Communication.Services;

public class MessageSender(IIamService IamService, IHubContext<ChatHub> hub, Persistence.Data.Messages messagesData, Persistence.Data.Profiles profilesData) : Interfaces.IMessageSender
{

    /// <summary>
    /// Enviar mensaje.
    /// </summary>
    public async Task<ResponseBase> SendDelay(int idTemporal, int profileId)
    {
        // Obtener los datos.
        var message = await messagesData.ReadOneTemp(idTemporal);
        var profile = await profilesData.Read(profileId);

        return await Send(new()
        {
            Contenido = message.Model.Message,
            Conversacion = new()
            {
                Id = message.Model.Conversation
            }
        }, string.Empty, profile.Model, null);
    }


    /// <summary>
    /// Enviar mensaje.
    /// </summary>
    /// <param name="message">Modelo del mensaje.</param>
    /// <param name="guid">New guid.</param>
    /// <param name="sender">Remitente.</param>
    public async Task<ResponseBase> Send(MessageModel message, string guid, JwtModel sender, DateTime? timeToSend = null)
    {
        return await Send(message, guid, new ProfileModel()
        {
            Id = sender.ProfileId,
            Alias = sender.Alias,
            IdentityId = sender.IdentityId,
        }, timeToSend);
    }


    /// <summary>
    /// Enviar mensaje.
    /// </summary>
    /// <param name="message">Modelo.</param>
    /// <param name="guid">Guid.</param>
    /// <param name="sender">Autenticación.</param>
    public async Task<ResponseBase> Send(MessageModel message, string guid, ProfileModel sender, DateTime? timeToSend = null)
    {

        // Validar modelo.
        if (message is null || string.IsNullOrWhiteSpace(message.Contenido))
            return new()
            {
                Message = "Message content can´t be empty",
                Response = Responses.InvalidParam,
            };

        // Si el mensaje es programado.
        if (timeToSend is not null)
        {

            // Obtener el delay.
            TimeSpan delay = timeToSend.Value - DateTime.Now;

            // Validar si el delay es valido.
            if (delay > TimeSpan.Zero)
            {
                // Crear el mensaje temporal.
                var createMessageTemp = await messagesData.Create(new TempMessageModel()
                {
                    Conversation = message.Conversacion.Id,
                    Message = message.Contenido,
                    Time = timeToSend.Value,
                });

                // Encolar el Job.
                BackgroundJob.Schedule<MessageSender>((t) => t.SendDelay(createMessageTemp.LastId, sender.Id), delay);

                return new()
                {
                    Response = Responses.Success,
                    Message = $"El mensaje fue programado para enviarse en {delay.Minutes} minutos."
                };
            }

            return new()
            {
                Message = "La fecha y hora deben estar en el futuro.",
                Response = Responses.InvalidParam,
                Errors = [new() {
                    Tittle = "Fecha y hora de envió invalida",
                    Description = "La fecha para programar el mensaje debe estar en el futuro."
                }]
            };
        }

        // Validar acceso Iam.
        IamLevels iam = await IamService.Validate(sender.Id, message.Conversacion.Id);

        if (iam == IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message.Contenido,
            Remitente = new()
            {
                IdentityId = sender.IdentityId,
                Alias = sender.Alias,
                Id = sender.Id
            },
            Time = DateTime.Now,
            Guid = guid,
            Conversacion = new()
            {
                Id = message.Conversacion.Id
            }
        };

        // Envía el mensaje en tiempo real.
        await hub.Clients.Group(message.Conversacion.Id.ToString()).SendAsync($"sendMessage", messageModel);

        // Crea el mensaje en la BD.
        await messagesData.Create(messageModel);

        // Retorna el resultado.
        return new()
        {
            Response = Responses.Success
        };

    }


    public async Task<ResponseBase> SendSystem(MessageModel message)
    {
        
        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message.Contenido,
            Remitente = null,
            Time = DateTime.Now,
            Guid = Guid.NewGuid().ToString(),
            Type = message.Type,
            Conversacion = new()
            {
                Id = message.Conversacion.Id
            }
        };

        // Envía el mensaje en tiempo real.
        await hub.Clients.Group(message.Conversacion.Id.ToString()).SendAsync($"sendMessage", messageModel);

        // Crea el mensaje en la BD.
        await messagesData.Create(messageModel);

        // Retorna el resultado.
        return new()
        {
            Response = Responses.Success
        };
    }
}