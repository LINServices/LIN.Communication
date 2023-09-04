namespace LIN.Communication.Data;


public class Messages
{


    #region Abstracciones



    /// <summary>
    /// Crea un nuevo mensaje
    /// </summary>
    /// <param name="data">Modelo del mensaje</param>
    public async static Task<CreateResponse> Create(MessageModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }




    /// <summary>
    /// Obtiene los mensajes asociados a una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación</param>
    /// <param name="lastID">ID mínimo para obtener</param>
    public async static Task<ReadAllResponse<MessageModel>> ReadAll(int id, int lastID)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, lastID, context);

        context.CloseActions(connectionKey);

        return response;

    }




    #endregion



    /// <summary>
    /// Crea un nuevo mensaje
    /// </summary>
    /// <param name="data">Modelo del mensaje</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(MessageModel data, Conexión context)
    {
        // ID
        data.ID = 0;

        // Ejecución
        try
        {

            context.DataBase.Attach(data.Conversacion);
            context.DataBase.Attach(data.Remitente);

            var res = context.DataBase.Mensajes.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch
        {
        }
        return new();
    }




    /// <summary>
    /// Obtiene los mensajes asociados a una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación</param>
    /// <param name="lastID">ID mínimo para obtener</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<MessageModel>> ReadAll(int id, int lastID, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var baseQuery = (from M in context.DataBase.Mensajes
                             where M.Conversacion.ID == id
                             && M.ID > lastID
                             orderby M.ID descending
                             select new MessageModel
                             {
                                 Contenido = M.Contenido,
                                 Conversacion = M.Conversacion,
                                 ID = M.ID,
                                 Remitente = M.Remitente,
                                 Time = M.Time
                             }).Take(100);

            // Grupos
            var groups = await baseQuery.OrderBy(A => A.ID).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }



}