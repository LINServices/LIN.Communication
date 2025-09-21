namespace LIN.Communication.Memory;

public class Sessions : Dictionary<int, MemorySession>
{

    /// <summary>
    /// Obtiene una session
    /// </summary>
    /// <param name="profile">Id del perfil</param>
    public new MemorySession? this[int profile]
    {
        get
        {
            var session = this.Where(T => T.Key == profile).FirstOrDefault();
            return session.Value;
        }
    }



    /// <summary>
    /// Obtiene una session de acuerdo
    /// </summary>
    /// <param name="connectionId">Id del dispositivo</param>
    public MemorySession? this[string connectionId]
    {
        get
        {
            var session = this.Where(T => T.Value.Devices.Any(t => t.ConnectionId == connectionId)).FirstOrDefault();
            return session.Value;
        }
    }



    /// <summary>
    /// Agrega una nueva sesión
    /// </summary>
    /// <param name="session">Modelo</param>
    public void Add(MemorySession session)
    {
        this.Add(session.Profile.Id, session);
    }


}