namespace LIN.Communication.Persistence.Data;

public class Messages(Context context)
{

    /// <summary>
    /// Crea un nuevo mensaje
    /// </summary>
    /// <param name="data">Modelo del mensaje</param>
    public async Task<CreateResponse> Create(MessageModel data)
    {
        // Id
        data.Id = 0;

        // Ejecución
        try
        {
            context.Attach(data.Conversacion);

            if (data.Remitente != null)
                context.Attach(data.Remitente);

            var res = context.Messages.Add(data);
            await context.SaveChangesAsync();
            return new(Responses.Success, data.Id);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Crea un nuevo mensaje temporal.
    /// </summary>
    /// <param name="data">Modelo del mensaje</param>
    public async Task<CreateResponse> Create(TempMessageModel data)
    {
        // Id
        data.Id = 0;

        // Ejecución
        try
        {
            var res = context.TempMessages.Add(data);
            await context.SaveChangesAsync();
            return new(Responses.Success, data.Id);
        }
        catch (Exception)
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

            // Consulta.
            var baseQuery = (from M in context.Messages
                             where M.Conversacion.Id == id
                             && M.Id > lastID
                             orderby M.Id descending
                             select new MessageModel
                             {
                                 Contenido = M.Contenido,
                                 Id = M.Id,
                                 Remitente = M.Remitente,
                                 Type = M.Type,
                                 Time = M.Time
                             }).Take(100);

            // Grupos
            var groups = await baseQuery.OrderBy(A => A.Id).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene un mensajes temporal.
    /// </summary>
    /// <param name="id">Id del mensaje</param>
    public async Task<ReadOneResponse<TempMessageModel>> ReadOneTemp(int id)
    {

        // Ejecución
        try
        {

            // Consulta.
            var message = await (from M in context.TempMessages
                                 where M.Id == id
                                 select M).FirstOrDefaultAsync();

            return (message is null) ? new(Responses.NotRows) : new(Responses.Success, message);

        }
        catch (Exception)
        {
        }
        return new();
    }

}