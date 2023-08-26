namespace LIN.Communication.Data;


public class Profiles
{



    #region Abstracciones


    public async static Task<CreateResponse> Create(ProfileModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }





    #endregion



    public async static Task<CreateResponse> Create(ProfileModel data, Conexión context)
    {
        // ID
        data.ID = 0;

        // Ejecución
        try
        {
            var res = context.DataBase.Profiles.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch 
        {
        }
        return new();
    }




}