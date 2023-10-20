using LIN.Communication.Memory;

namespace LIN.Communication.Hubs;


public partial class ChatHub : Hub
{

    public static Dictionary<int, List<MessageModel>> Conversations { get; set; } = new();






    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public void Load(ProfileModel profile)
    {
        try
        {

            // Perfil actual.
            MemorySession? memorySession = Mems.Sessions[profile.ID];

            // Si no existe la sesión.
            if (memorySession == null)
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
    /// <param name="name">ID del grupo</param>
    public async Task JoinGroup(int name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, name.ToString());
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
    public async Task SendMessage(int me, int groupName, string message)
    {

        // Si el mansaje esta vacío.
        if (message.Trim() == string.Empty)
            return;

        // Data
        var data = Mems.Sessions[me];

        var conversationOnMemory = Conversations.Where(T => T.Key == groupName).FirstOrDefault().Value;

        List<MessageModel>? mensajes = conversationOnMemory;
        if (mensajes == null)
        {
            mensajes = new List<MessageModel>();
            Conversations.Add(groupName, mensajes);
        }

        // Obtiene el perfil.
        ProfileModel? profile = data?.Profile;

        // Si el perfil no existe, o esta registrado.
        if (profile == null)
            return;

        // Modelo del mensaje.
        MessageModel messageModel = new()
        {
            Contenido = message,
            Remitente = profile,
            Time = DateTime.Now,
            Conversacion = new()
            {
                ID = groupName
            }
        };

        // Envía el mensaje en tiempo real.

        await Clients.Group(groupName.ToString()).SendAsync($"sendMessage", messageModel);


        if (message.Contains("@emma"))
        {
            var emma = new Access.OpenIA.IA(Configuration.GetConfiguration("openIa:key"));

            // Carga el modelo
            emma.LoadWho();
            emma.LoadRecomendations();
            emma.LoadPersonality();
            emma.LoadSomething($""" 
                           Importante, en este momento estas un chat/grupo o conversación con una o mas personas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
                           el usuario probablemente te halla etiquetado, asi que deveras contestar como si fueras un integrante mas del grupo
                           """);

            emma.LoadSomething($""" 
                           Estos son los mensajes de contexto:
                           {ContextToEmma(mensajes)}
                           """);

            var result = await emma.Respond(message);


            MessageModel mensajeEmma = new()
            {
                Contenido = result.Result,
                Remitente = new()
                {
                    Alias = "Emma Asistente in Chat",
                    ID = -1
                },
                Time = DateTime.Now,
                Conversacion = new()
                {
                    ID = groupName
                }
            };

            mensajes.Add(messageModel);
            mensajes.Add(mensajeEmma);
            await Clients.Group(groupName.ToString()).SendAsync($"sendMessage", mensajeEmma);


        }
        else
        {
            mensajes.Add(messageModel);
        }



        // Establece el ID de la conversación

        // Crea el mensaje en la BD
        await Data.Messages.Create(messageModel);

    }




    string ContextToEmma(List<MessageModel> messages)
    {
        var lasts = messages.TakeLast(8).ToList();

        string content = "";

        foreach (var message in lasts)
        {
            string alias = message.Remitente.ID == -1 ? "tu respondiste" : $"{message.Remitente.Alias} dijo";
            content += $"{alias} <<<{message.Contenido}>>>.";
        }

        return content;
    }
}