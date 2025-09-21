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
    public List<(int, string)> Conversations { get; set; }


    /// <summary>
    /// Lista de dispositivos conectados
    /// </summary>
    public List<DeviceOnAccountModel> Devices { get; set; }


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
        Conversations = [];
        Devices = [];
    }


    /// <summary>
    /// Obtiene un string con la concatenación de los nombres de las conversaciones.
    /// </summary>
    public string StringOfConversations()
    {
        string final = "Estas son las conversaciones / chats del usuario: \n";

        foreach (var conversation in Conversations)
            final += $$"""
                      {
                           "id" : {{conversation.Item1}},
                           "name" : "{{conversation.Item2}}"
                      }
                      """;

        return final + "Recuerda que las conversaciones son grupos, chats, personas u cualquier otra palabra relacionada.";
    }

}