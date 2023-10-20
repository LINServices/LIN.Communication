namespace LIN.Communication.Hubs;


public class ChatHub : Hub
{


    public class Proff
    {
        public ProfileModel Profile { get; set; }
        public List<string> Conversations { get; set; } = new();
        public List<string> Devices { get; set; }
        public DateTime LastTime { get; set; }


        public string GetStringOfConversations()
        {
            string final = "";
            foreach(var s in Conversations)
            {
                final += $"'{s}',";
            }
            return final ;
        }
    }


    /// <summary>
    /// Lista perfiles.
    /// </summary>
    public static readonly Dictionary<int, Proff> Profiles = new();






    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {

            var count = Profiles.Where(T => T.Value.Devices.Contains(this.Context.ConnectionId)).FirstOrDefault();

            count.Value.LastTime = DateTime.Now;

            _ = Data.Profiles.SetLastConnection(count.Key, DateTime.Now);

            count.Value.Devices.Remove(this.Context.ConnectionId);
        }
        catch
        {
        }
    }




    /// <summary>
    /// Agrega a el grupo
    /// </summary>
    public void Load(ProfileModel profile)
    {
        try
        {

            Proff proff = new Proff();

            var exist = Profiles.Where(T => T.Key == profile.ID).FirstOrDefault();
            if (exist.Key == 0)
            {
                proff = new Proff()
                {
                    Profile = profile,
                    Devices = new List<string>()
                };

                Profiles.Add(profile.ID, proff
               );
            }
            else
            {
                proff = exist.Value;
            }

            proff.Devices.Add(this.Context.ConnectionId);

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
        var data = Profiles.Where(P => P.Key == me).FirstOrDefault().Value;

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

        
        if (message.Contains("@emma"))
        {
            var emma = new Access.OpenIA.IA(Configuration.GetConfiguration("openIa:key"));

            // Carga el modelo
            emma.LoadWho();
            emma.LoadRecomendations();
            emma.LoadCommands();
            emma.LoadPersonality();

            emma.LoadSomething($""" 
                           Estas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
                           Estos son los nombres de los chats que tiene el usuario: {data?.GetStringOfConversations()}
                           Recuerda que si el usuario quiere mandar un mensaje a un usuario/grupo/team etc, primero busca en su lista de nombres de chats
                           """);

            var result = await emma.Respond(message);

            messageModel.Contenido = result.Result;
        }

        // Envía el mensaje en tiempo real.
        await Clients.Group(groupName.ToString()).SendAsync($"sendMessage", messageModel);



        // Establece el ID de la conversación

        // Crea el mensaje en la BD
        await Data.Messages.Create(messageModel);

    }


}