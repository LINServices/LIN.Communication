namespace LIN.Communication.Data;


public class Conversations
{



    #region Abstracciones


    public async static Task<CreateResponse> Create(ConversaciónModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    public async static Task<ReadAllResponse<ConversaciónModel>> ReadAll(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, context);

        context.CloseActions(connectionKey);

        return response;

    }






    #endregion





    public async static Task<CreateResponse> Create(ConversaciónModel data, Conexión context)
    {
        // ID
        data.ID = 0;

        // Ejecución
        try
        {
            var res = context.DataBase.Conversaciones.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch 
        {
        }
        return new();
    }


    public async static Task<ReadAllResponse<ConversaciónModel>> ReadAll(int id, Conexión context)
    {
     

        // Ejecución
        try
        {

            var con = await (from P in context.DataBase.Conversaciones
                                 where P.UsuarioAID == id
                                 || P.UsuarioBID == id
                                 select P).DistinctBy(A=>A.ID).ToListAsync();

            var lista = con;


            return new(Responses.Success, lista ?? new());
        }
        catch
        {
        }
        return new();
    }





}