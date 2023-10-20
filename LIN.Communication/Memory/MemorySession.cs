namespace LIN.Communication.Memory;


public class MemorySession
{

    /// <summary>
    /// Perfil.
    /// </summary>
    public ProfileModel Profile { get; set; }


    /// <summary>
    /// Lista de nombres de los chats.
    /// </summary>
    public List<string> Conversations { get; set; }


    /// <summary>
    /// Lista de dispositivos conectados
    /// </summary>
    public List<string> Devices { get; set; }


    /// <summary>
    /// Hora de la ultima conexión.
    /// </summary>
    public DateTime LastTime { get; set; }



    /// <summary>
    /// Nueva session en memoria.
    /// </summary>
    public MemorySession()
    {
        Profile = new ProfileModel();
        Conversations = new List<string>();
        Devices = new List<string>();
    }


    public string StringOfConversations()
    {
        string final = "";
        foreach (var conversation in Conversations)
        {
            final += $"'{conversation}',";
        }
        return final;
    }

}