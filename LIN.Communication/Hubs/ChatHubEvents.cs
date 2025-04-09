namespace LIN.Communication.Hubs;

public partial class ChatHub : Hub
{

    /// <summary>
    /// Evento: Cuando un dispositivo se desconecta
    /// </summary>
    /// <param name="exception">Excepción</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            // Obtiene la sesión por el dispositivo
            var session = Mems.Sessions[Context.ConnectionId];

            // No existe.
            if (session is null)
                return;

            // Remover el dispositivo.
            session.Devices.RemoveAll(T => T == Context.ConnectionId);
            session.LastTime = DateTime.Now;

            // Establece la ultima conexión.
            await profilesData.SetLastConnection(session.Profile.Id, DateTime.Now);

        }
        catch (Exception)
        {
        }
    }

}