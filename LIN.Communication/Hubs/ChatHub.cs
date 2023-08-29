namespace LIN.Communication.Hubs;


public class ChatHub : Hub
{


    /// <summary>
    /// Lista perfiles.
    /// </summary>
    public static readonly HashSet<ProfileModel> Profiles = new();



    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public void Load(ProfileModel profile)
    {
        try
        {
            var exist = Profiles.Where(T => T.ID == profile.ID).Any();
            if (!exist)
                Profiles.Add(profile);
        }
        catch
        {
        }
    }



    /// <summary>
    /// Une una conexión a un grupo de tiempo real.
    /// </summary>
    /// <param name="name">ID del grupo</param>
    public async Task JoinGroup(string name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, name);
    }



    /// <summary>
    /// Elimina un usuario de un grupo.
    /// </summary>
    /// <param name="name">ID del grupo</param>
    public async Task LeaveGroup(string name)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, name);
    }



    /// <summary>
    /// Enviar un mensaje.
    /// </summary>
    /// <param name="me">ID del perfil</param>
    /// <param name="groupName">ID del grupo</param>
    /// <param name="message">Mensaje</param>
    public async Task SendMessage(int me, string groupName, string message)
    {

        // Si el mansaje esta vacio.
        if (message.Trim() == string.Empty)
            return;

        // Obtiene el perfil.
        ProfileModel? profile = Profiles.Where(P => P.ID == me).FirstOrDefault();

        // Si el perfil no existe, o esta registrado.
        if (profile == null)
            return;

        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message,
            Remitente = profile,
            Time = DateTime.Now
        };

        // Envía el mensaje en tiempo real.
        await Clients.Group(groupName).SendAsync($"sendMessage-{groupName}", messageModel);

        // Establece el ID de la conversación
        messageModel.Conversacion = new()
        {
            ID = int.Parse(groupName)
        };

        // Crea el mensaje en la BD
        await Data.Messages.Create(messageModel);

    }


}