using Microsoft.AspNetCore.SignalR;

namespace LIN.Communication.Hubs;


public class ChatHub : Hub
{


    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }



    /// <summary>
    /// Elimina un usuario de un grupo
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }



    /// <summary>
    /// Agrega un nuevo producto
    /// </summary>
    public async Task SendMessage(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("sendMessage", message);
    }


}