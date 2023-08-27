namespace LIN.Communication.Hubs;


public class ChatHub : Hub
{




    public static List<ProfileModel> Profiles = new();

    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public async Task Load(ProfileModel profile)
    {
        var exist = Profiles.Where(T => T.ID == profile.ID).Any();
        if (!exist)
            Profiles.Add(profile);
    }




    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public async Task JoinGroup(string name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, name);
    }



    /// <summary>
    /// Elimina un usuario de un grupo
    /// </summary>
    public async Task LeaveGroup(string name)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, name);
    }



    /// <summary>
    /// Agrega un nuevo producto
    /// </summary>
    public async Task SendMessage(int me, string groupName, string message)
    {
        var Me = Profiles.Where(T => T.ID == me).FirstOrDefault();
        if (Me != null)
        {
            var messageModel = new MessageModel()
            {
                Contenido = message,
                Remitente = Me,
                Time = DateTime.Now
            };
            await Clients.Group(groupName).SendAsync($"sendMessage-{groupName}", messageModel);
        }

    }


}