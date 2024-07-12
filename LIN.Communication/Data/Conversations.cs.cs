namespace LIN.Communication.Data;


public partial class Conversations
{


    /// <summary>
    /// Crea una conversación (Grupo).
    /// </summary>
    /// <param name="data">Modelo</param>
    /// <param name="context">Contexto de conexión.</param>
    public async Task<CreateResponse> Create(ConversationModel data, Conexión context)
    {
        // Id
        data.ID = 0;

        // Ejecución
        try
        {

            foreach (var user in data.Members)
                context.DataBase.Attach(user.Profile);

            var res = context.DataBase.Conversaciones.Add(data);
            await context.DataBase.SaveChangesAsync();
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
    /// <param name="context">Contexto de conexión.</param>
    public async Task<ReadAllResponse<MemberChatModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members
                                where M.Profile.ID == id
                                where M.Conversation.Visibility == ConversationVisibility.@public
                                select new MemberChatModel
                                {
                                    Conversation = new ConversationModel
                                    {
                                        ID = M.Conversation.ID,
                                        Name = (M.Conversation.Type != ConversationsTypes.Personal) ? M.Conversation.Name
                                               : M.Conversation.Members.FirstOrDefault(t => t.Profile.ID != id)!.Profile.Alias ?? "Yo",
                                        Type = M.Conversation.Type,
                                        Visibility = M.Conversation.Visibility
                                    },
                                    Rol = M.Rol
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async Task<ReadOneResponse<MemberChatModel>> Read(int id, Conexión context, int profileContext = 0)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members
                                where M.Conversation.ID == id
                                && M.Conversation.Visibility == ConversationVisibility.@public
                                select new MemberChatModel
                                {
                                    Conversation = new ConversationModel
                                    {
                                        ID = M.Conversation.ID,
                                        Name = (M.Conversation.Type != ConversationsTypes.Personal) ? M.Conversation.Name
                                   : M.Conversation.Members.FirstOrDefault(t => t.Profile.ID != profileContext)!.Profile.Alias ?? "Yo",

                                        Type = M.Conversation.Type,
                                        Visibility = M.Conversation.Visibility
                                    },
                                    Rol = M.Rol
                                }).FirstOrDefaultAsync();

            if (groups == null)
                return new(Responses.NotRows);

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }



    /// <summary>
    /// Actualizar nombre.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="name">Nuevo nombre de la conversación.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async Task<ResponseBase> UpdateName(int id, string name, Conexión context)
    {

        // Ejecución
        try
        {
            // Consulta
            var v = await (from M in context.DataBase.Conversaciones
                           where M.ID == id
                           where M.Type != ConversationsTypes.Personal
                           select M).ExecuteUpdateAsync(setters => setters
                           .SetProperty(b => b.Name, name));


            if (v <= 0)
                return new(Responses.NotRows);

            return new(Responses.Success);
        }
        catch
        {
        }
        return new();
    }


}