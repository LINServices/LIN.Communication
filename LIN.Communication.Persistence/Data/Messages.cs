namespace LIN.Communication.Persistence.Data;

public class Messages (Context context)
{

    /// <summary>
    /// Crea un nuevo mensaje
    /// </summary>
    /// <param name="data">Modelo del mensaje</param>
    public async Task<CreateResponse> Create(MessageModel data)
    {
        // Id
        data.ID = 0;

        // Ejecución
        try
        {

            context.Attach(data.Conversacion);
            context.Attach(data.Remitente);

            var res = context.Mensajes.Add(data);
            await context.SaveChangesAsync();
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
    /// <param name="id">Id de la conversación</param>
    /// <param name="lastID">Id mínimo para obtener</param>
    public async Task<ReadAllResponse<MessageModel>> ReadAll(int id, int lastID)
    {

        // Ejecución
        try
        {

            // Consulta
            var baseQuery = (from M in context.Mensajes
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
        catch (Exception)
        {
        }
        return new();
    }

}