using LIN.Communication.Services.Interfaces;

namespace LIN.Communication.Hubs;

public partial class ChatHub(IMessageSender messageSender, Persistence.Data.Profiles profilesData) : Hub
{

    /// <summary>
    /// Carga una sesión.
    /// </summary>
    /// <param name="profile">Perfil</param>
    public void Load(ProfileModel profile)
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

            // Agrega el dispositivo actual.
            memorySession.Devices.Add(Context.ConnectionId);

        }
        catch
        {
        }
    }


    /// <summary>
    /// Une una conexión a un grupo de tiempo real.
    /// </summary>
    /// <param name="name">Id del grupo</param>
    public async Task JoinGroup(int name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, name.ToString());
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
    /// Enviar un mensaje.
    /// </summary>
    /// <param name="profileId">Id del perfil</param>
    /// <param name="groupName">Id del grupo</param>
    /// <param name="message">Mensaje</param>
    public async Task SendMessage(int profileId, int groupName, string message, string guid)
    {

        // Si el mansaje esta vacío.
        if (message.Trim() == string.Empty)
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

        await messageSender.Send(messageModel, guid, profile);

    }

}