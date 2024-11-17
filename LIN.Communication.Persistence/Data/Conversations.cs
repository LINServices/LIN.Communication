namespace LIN.Communication.Persistence.Data;

public class Conversations(Context context)
{

    /// <summary>
    /// Crea una conversación (Grupo).
    /// </summary>
    /// <param name="data">Modelo</param>
    public async Task<CreateResponse> Create(ConversationModel data)
    {
        // Id
        data.ID = 0;

        // Ejecución
        try
        {
            foreach (var user in data.Members)
                context.Attach(user.Profile);

            var res = context.Conversations.Add(data);
            await context.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.Members
                                where M.Profile.ID == id
                                where M.Conversation.Visibility == ConversationVisibility.@public
                                select new MemberChatModel
                                {
                                    Conversation = new ConversationModel
                                    {
                                        ID = M.Conversation.ID,
                                        Name = M.Conversation.Type != ConversationsTypes.Personal ? M.Conversation.Name
                                               : M.Conversation.Members.FirstOrDefault(t => t.Profile.ID != id)!.Profile.Alias ?? "Yo",
                                        Type = M.Conversation.Type,
                                        Visibility = M.Conversation.Visibility
                                    },
                                    Rol = M.Rol
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async Task<ReadOneResponse<MemberChatModel>> Read(int id, int profileContext = 0)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.Members
                                where M.Conversation.ID == id
                                && M.Conversation.Visibility == ConversationVisibility.@public
                                select new MemberChatModel
                                {
                                    Conversation = new ConversationModel
                                    {
                                        ID = M.Conversation.ID,
                                        Name = M.Conversation.Type != ConversationsTypes.Personal ? M.Conversation.Name
                                   : M.Conversation.Members.FirstOrDefault(t => t.Profile.ID != profileContext)!.Profile.Alias ?? "Yo",

                                        Type = M.Conversation.Type,
                                        Visibility = M.Conversation.Visibility
                                    },
                                    Rol = M.Rol
                                }).FirstOrDefaultAsync();

            if (groups is null)
                return new(Responses.NotRows);

            return new(Responses.Success, groups);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Actualizar nombre.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="name">Nuevo nombre de la conversación.</param>
    public async Task<ResponseBase> UpdateName(int id, string name)
    {

        // Ejecución
        try
        {
            // Consulta
            var v = await (from M in context.Conversations
                           where M.ID == id
                           where M.Type != ConversationsTypes.Personal
                           select M).ExecuteUpdateAsync(setters => setters
                           .SetProperty(b => b.Name, name));


            if (v <= 0)
                return new(Responses.NotRows);

            return new(Responses.Success);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Encontrar una conversación.
    /// </summary>
    /// <param name="friendId">Id del amigo.</param>
    /// <param name="profileId">Id propio.</param>
    public async Task<ReadOneResponse<ConversationModel>> Find(int friendId, int profileId)
    {
        var conversation = await (from u in context.Conversations
                                  where u.Type == ConversationsTypes.Personal
                                  && u.Members.Count == 2
                                  && u.Members.Where(t => t.Profile.ID == friendId).Any()
                                  && u.Members.Where(t => t.Profile.ID == profileId).Any()
                                  select u).FirstOrDefaultAsync();

        return new(conversation!);
    }

}