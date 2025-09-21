namespace LIN.Communication.Hubs;

public partial class ChatHub(IMessageSender messageSender, Persistence.Data.Profiles profilesData) : Hub
{

    /// <summary>
    /// Crear una sesión de tiempo real a partir de un perfil.
    /// </summary>
    /// <param name="profile">Modelo del perfil.</param>
    public void Load(ProfileModel profile, DeviceOnAccountModel device)
    {
        try
        {
            // Perfil actual.
            MemorySession? memorySession = Mems.Sessions[profile.Id];

            // Si no existe la sesión.
            if (memorySession is null)
            {
                // Modelo.
                memorySession = new()
                {
                    Profile = profile
                };

                // Agrega la sesión.
                Mems.Sessions.Add(memorySession);
            }

            device.ConnectionId = Context.ConnectionId;

            // Agrega el dispositivo actual.
            memorySession.Devices.Add(device);

        }
        catch
        {
        }
    }


    /// <summary>
    /// Unir un dispositivo a un grupo.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async Task JoinGroup(int id)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
    }


    /// <summary>
    /// Elimina un usuario de un grupo.
    /// </summary>
    /// <param name="name">Id del grupo</param>
    public async Task LeaveGroup(string name)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, name);
    }

    /// <summary>
    /// Enviar comando a un dispositivo.
    /// </summary>
    /// <param name="device">Id del dispositivo.</param>
    /// <param name="command">Comando a ejecutar.</param>
    public async Task SendToDevice(string device, string command)
    {
        // Envía el comando.
        await Clients.Client(device).SendAsync("#command", command);
    }

    /// <summary>
    /// Enviar un mensaje.
    /// </summary>
    /// <param name="profileId">Id del perfil</param>
    /// <param name="groupName">Id del grupo</param>
    /// <param name="message">Mensaje</param>
    public async Task SendMessage(int profileId, int groupName, string message, string guid, DateTime? timeToSend = null)
    {
        // Si el mansaje esta vacío.
        if (string.IsNullOrWhiteSpace(message))
            return;

        // Data.
        var data = Mems.Sessions[profileId];

        // Obtiene el perfil.
        ProfileModel? profile = data?.Profile;

        // Si el perfil no existe, o esta registrado.
        if (profile is null)
            return;

        // Hora actual.
        var time = DateTime.Now;

        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message,
            Remitente = profile,
            Time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0),
            Guid = guid,
            Conversacion = new()
            {
                Id = groupName
            }
        };

        await messageSender.Send(messageModel, guid, profile, timeToSend);
    }

}