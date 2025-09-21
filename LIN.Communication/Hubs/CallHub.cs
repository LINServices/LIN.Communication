using LIN.Communication.Persistence.Data;

namespace LIN.Communication.Hubs;

public class CallHub(Members members, Meetings meetings, IHubContext<ChatHub> hub, IMessageSender messageSender) : Hub
{

    public async Task Join(int conversation, string token)
    {
        var tokenInfo = Jwt.Validate(token);

        if (tokenInfo.IsAuthenticated == false)
        {
            await Clients.Caller.SendAsync("Error", "Token inválido");
            return;
        }

        // Validar accceso Iam sobre la conversación.
        // TODO.....

        // Validar si hay una llamada en progreso.
        var actualConversation = await meetings.FindMeet(conversation);

        if (actualConversation.Response is Responses.NotRows)
        {
            // Crear la llamada y enviar notificacion de llamada a los clientes.
            actualConversation.Model = new()
            {
                StartTime = DateTime.UtcNow,
                EndTime = null,
                ConversationId = conversation
            };

            var create = await meetings.AddMeeting(actualConversation.Model);

            // generar mensaje de llamada.
            await messageSender.SendSystem(new()
            {
                Contenido = "Nueva llamada",
                Type = MessageTypes.Call,
                Conversacion = new()
                {
                    Id = conversation
                }
            });

        }
        // Notificar a los clientes actuales una persona nueva.
        await Clients.Group($"{conversation}").SendAsync("PeerJoined", Context.ConnectionId);

        // Buscar el miembro de la llamada.
        var member = await meetings.FindMeetMember(conversation, tokenInfo.ProfileId);

        // Si no, crearlo.
        if (member.Response is Responses.NotRows)
        {
            var newMember = await meetings.AddMember(new()
            {
                Devices = [],
                Meeting = actualConversation.Model,
                ProfileModel = new() { Id = tokenInfo.ProfileId }
            });

            if (newMember.Response is not Responses.Success)
            {
                return;
            }

            member = await meetings.FindMeetMember(conversation, tokenInfo.ProfileId);
        }
        else if (member.Response is Responses.Success)
        {

        }
        else
        {
            return;
        }

        // Sobre el miembro, agregar el dispositivo.
        await meetings.AddDevice(new()
        {
            MeetingMember = member.Model,
            DeviceName = "Testing",
            DeviceIdentifier = Context.ConnectionId
        });

        await Groups.AddToGroupAsync(Context.ConnectionId, $"{conversation}");

        var devices = await meetings.ReadDevices(conversation);

        await Clients.Caller.SendAsync("PeersInRoom", devices.Models.Where(t => t.DeviceIdentifier != Context.ConnectionId).Select(t => t.DeviceIdentifier));

        // Lllamar a los otros integrantes, si somos los primeros.
        if (devices.Models.Where(t => t.MeetingMemberId != member.Model.Id).Count() == 1)
        {
            await hub.Clients.Group($"{conversation}").SendAsync("UserInCall", conversation);
        }
    }













    public async Task Leave()
    {
        await OnDisconnectedAsync(null);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        // Buscar dispositivo por Id de conexion signalR
        var device = await meetings.ReadDeviceBySignal(Context.ConnectionId); // TODO: Pasar el id de la conversación.

        if (device.Response == Responses.NotRows)
        {
            return;
        }

        // Eliminar el dispositivo.
        await meetings.DeleteDevice(device.Model.Id);

        // Notificar a otros que alguien salió
        await Clients.Group($"{device.Model.MeetingMember.Meeting.ConversationId}").SendAsync("PeerLeft", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{device.Model.MeetingMember.Meeting.ConversationId}");

        var devices = await meetings.ReadDevices(device.Model.MeetingMember.Meeting.ConversationId);

        // Limpiar si la sala quedó vacía
        if (devices.Models.Count is 0)
        {
            // Finalizar llamada.
            await meetings.FinalizeCall(device.Model.MeetingMember.Meeting.Id);

            // generar mensaje de llamada.
            await messageSender.SendSystem(new()
            {
                Contenido = "Finalizar llamada",
                Type = MessageTypes.Call,
                Conversacion = new()
                {
                    Id = device.Model.MeetingMember.Meeting.ConversationId
                }
            });
        }
    }

    public Task SendSdp(string targetConnectionId, string type, string sdp) =>
        Clients.Client(targetConnectionId).SendAsync("Sdp", Context.ConnectionId, type, sdp);

    public Task SendIce(string targetConnectionId, string candidate, string sdpMid, int sdpMLineIndex) =>
        Clients.Client(targetConnectionId).SendAsync("Ice", Context.ConnectionId, candidate, sdpMid, sdpMLineIndex);
}
