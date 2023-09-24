namespace LIN.Communication.Data;


public class Conversations
{


    #region Abstracciones


    /// <summary>
    /// Crea una conversación (Grupo).
    /// </summary>
    /// <param name="data">Modelo</param>
    public async static Task<CreateResponse> Create(ConversationModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil
    /// </summary>
    /// <param name="id">ID del perfil.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene los miembros asociadas a una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadMembers(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadMembers(id, context);

        context.CloseActions(connectionKey);

        return response;

    }




    public async static Task<ResponseBase> HaveAccessFor(int profile, int conversation)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await HaveAccessFor(profile, conversation, context);

        context.CloseActions(connectionKey);

        return response;

    }



    #endregion



    /// <summary>
    /// Crea una conversación (Grupo).
    /// </summary>
    /// <param name="data">Modelo</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<CreateResponse> Create(ConversationModel data, Conexión context)
    {
        // ID
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
        catch
        {
        }
        return new();
    }



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil
    /// </summary>
    /// <param name="id">ID del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members
                                where M.Profile.ID == id
                                select new MemberChatModel
                                {
                                    Conversation = M.Conversation,
                                    Rol = M.Rol,
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }






    public async static Task<ResponseBase> HaveAccessFor(int profile, int conversation, Conexión context)
    {

        // Ejecución
        try
        {

            var have = await (from PA in context.DataBase.Members
                              where PA.Profile.ID == profile
                              && PA.Conversation.ID == conversation
                              select PA).AnyAsync();

            return new((have) ? Responses.Success : Responses.NotRows);
        }
        catch
        {
        }
        return new();
    }





    /// <summary>
    /// Obtiene los miembros asociadas a una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadMembers(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members
                                where M.Conversation.ID == id
                                select new MemberChatModel
                                {
                                    Profile = new()
                                    {
                                        Alias = M.Profile.Alias,
                                        ID = M.Profile.ID,
                                        AccountID = M.Profile.ID
                                    },
                                    Rol = M.Rol,
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }


}