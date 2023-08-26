namespace LIN.Communication.Data;


public class Profiles
{



    #region Abstracciones


    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    public async static Task<ReadOneResponse<ProfileModel>> Read(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Read(id, context);

        context.CloseActions(connectionKey);

        return response;

    }


    public async static Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadByAccount(id, context);

        context.CloseActions(connectionKey);

        return response;

    }




    #endregion





    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data, Conexión context)
    {
        // ID
        data.Profile.ID = 0;

        // Ejecución
        try
        {
            var res = context.DataBase.Profiles.Add(data.Profile);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.Profile);
        }
        catch 
        {
        }
        return new();
    }


    public async static Task<ReadOneResponse<ProfileModel>> Read(int id, Conexión context)
    {
     

        // Ejecución
        try
        {

            var profile = await (from P in context.DataBase.Profiles
                                 where P.ID == id
                                 select P).FirstOrDefaultAsync();

            return new(Responses.Success, profile ?? new());
        }
        catch
        {
        }
        return new();
    }



    public async static Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id, Conexión context)
    {


        // Ejecución
        try
        {

            var profile = await (from P in context.DataBase.Profiles
                                 where P.AccountID == id
                                 select P).FirstOrDefaultAsync();

            if (profile == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, profile ?? new());
        }
        catch
        {
        }
        return new();
    }




}