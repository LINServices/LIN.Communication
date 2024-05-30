namespace LIN.Communication.Data;

public interface IConversations
{

    /// <summary>
    /// Crea una conversación.
    /// </summary>
    /// <param name="data">Modelo</param>
    public Task<CreateResponse> Create(ConversationModel data);



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public Task<ReadAllResponse<MemberChatModel>> ReadAll(int id);



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public Task<ReadOneResponse<MemberChatModel>> Read(int id, int profileContext = 0, bool useCache = false);



    /// <summary>
    /// Actualizar nombre.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="name">Nuevo nombre.</param>
    public Task<ResponseBase> UpdateName(int id, string name);



    /// <summary>
    /// Crea una conversación (Grupo).
    /// </summary>
    /// <param name="data">Modelo</param>
    /// <param name="context">Contexto de conexión.</param>
    public Task<CreateResponse> Create(ConversationModel data, Conexión context);



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public Task<ReadAllResponse<MemberChatModel>> ReadAll(int id, Conexión context);



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="context">Contexto de conexión.</param>
    public Task<ReadOneResponse<MemberChatModel>> Read(int id, Conexión context, int profileContext = 0);



    /// <summary>
    /// Actualizar nombre.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="name">Nuevo nombre de la conversación.</param>
    /// <param name="context">Contexto de conexión.</param>
    public Task<ResponseBase> UpdateName(int id, string name, Conexión context);




}