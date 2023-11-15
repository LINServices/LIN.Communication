namespace LIN.Communication.Hubs;


public partial class ChatHub : Hub
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
    public async Task SendMessage(int me, int groupName, string message, string guid)
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
            mensajes = [];
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
            Guid = guid,
            Conversacion = new()
            {
                ID = groupName
            }
        };

        // Envía el mensaje en tiempo real.
        await Clients.Group(groupName.ToString()).SendAsync($"sendMessage", messageModel);


        if (message.Contains("@emma"))
        {

            // Modelo de Emma.
            var modelIA = new Access.OpenIA.IAModelBuilder(Configuration.GetConfiguration("openIa:key"));

            // Cargar el modelo
            modelIA.Load(IA.IAConsts.Base);
            modelIA.Load(IA.IAConsts.Personalidad);
            modelIA.Load($""" 
                           Importante, en este momento estas un chat/grupo o conversación con una o mas personas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
                           el usuario probablemente te halla etiquetado, asi que deveras contestar como si fueras un integrante mas del grupo y debes de tener de contexto los mensajes del usuario y de los otros usuarios
                           """);

            // Mensajes
            var lastMessages = mensajes.TakeLast(8);

            string chatHistory = "";
            foreach (var lastMessage in lastMessages)
                chatHistory += $"'{lastMessage.Remitente.Alias}' ha enviado el mensaje '{lastMessage.Contenido}', ";

            modelIA.Load($""" 
                          Este es el historial de chat de la conversación, recuerda analizar minuciosamente, recuerda que tu eres Emma, el usuario se llama '{profile.Alias}' y los demás son integrantes de la conversación.
                           Historial:{chatHistory}
                          """);

            var response = await modelIA.Reply(message);

            MessageModel mensajeEmma = new()
            {
                Contenido = response.Content,
                Remitente = new()
                {
                    Alias = "Emma assistant in Chat",
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

        // Crea el mensaje en la BD
        await Data.Messages.Create(messageModel);

    }



    public static Dictionary<int, List<MessageModel>> Conversations { get; set; } = [];
  
}