namespace LIN.Communication.Data;


public partial class Conversations : IConversations
{


    /// <summary>
    /// Crea una conversación.
    /// </summary>
    /// <param name="data">Modelo</param>
    public async Task<CreateResponse> Create(ConversationModel data)
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
    /// <param name="id">Id del perfil.</param>
    public async Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async Task<ReadOneResponse<MemberChatModel>> Read(int id, int profileContext = 0, bool useCache = false)
    {
        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Read(id, context, profileContext);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Actualizar nombre.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="name">Nuevo nombre.</param>
    public async Task<ResponseBase> UpdateName(int id, string name)
    {
        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await UpdateName(id, name, context);

        context.CloseActions(connectionKey);

        return response;

    }


}