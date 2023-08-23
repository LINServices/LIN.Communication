using Microsoft.AspNetCore.SignalR;

namespace LIN.Communication.Hubs;


public class ChatHub : Hub
{



    public ProfileModel Me { get; set; } = new();



    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public async Task Load(string alias)
    {
        Me = new()
        {
            Alias = alias
        };
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
    public async Task SendMessage(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("sendMessage", Me.Alias ?? "Unknow", message ?? "");
    }


}