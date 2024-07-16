namespace LIN.Communication.Data;


public partial class Members
{


    /// <summary>
    /// Agregar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    public static async Task<ResponseBase> Create(int id, int profile)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(id, profile, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene los miembros asociadas a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public static async Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// EEliminar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    public static async Task<ResponseBase> Remove(int id, int profile)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Remove(id, profile, context);

        context.CloseActions(connectionKey);

        return response;

    }



}