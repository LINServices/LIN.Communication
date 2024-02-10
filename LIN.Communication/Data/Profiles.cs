namespace LIN.Communication.Data;


public class Profiles
{



    #region Abstracciones


    /// <summary>
    /// Crea un perfil.
    /// </summary>
    /// <param name="data">Modelo.</param>
    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data)
    {

        // Contexto de conexión.
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene un perfil.
    /// </summary>
    /// <param name="id">Id del perfil</param>
    public async static Task<ReadOneResponse<ProfileModel>> Read(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Read(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene un perfil por medio del Id de su cuenta.
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    public async static Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadByAccount(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene una lista de usuarios según los Ids de las cuentas.
    /// </summary>
    /// <param name="ids">Ids de las cuentas</param>
    public async static Task<ReadAllResponse<ProfileModel>> ReadByAccounts(IEnumerable<int> ids)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadByAccounts(ids, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtener la ultima conexión.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async static Task<ReadOneResponse<DateTime>> GetLastConnection(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await GetLastConnection(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Establecer la ultima conexión.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="time">Hora de conexión</param>
    public async static Task<ResponseBase> SetLastConnection(int id, DateTime time)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await SetLastConnection(id, time, context);

        context.CloseActions(connectionKey);

        return response;

    }



    #endregion



    /// <summary>
    /// Crea un perfil.
    /// </summary>
    /// <param name="data">Modelo.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data, Conexión context)
    {
        // Id
        data.Profile.ID = 0;
        data.Profile.Friends = [];

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



    /// <summary>
    /// Obtiene un perfil.
    /// </summary>
    /// <param name="id">Id del perfil</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<ProfileModel>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta.
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



    /// <summary>
    /// Obtiene un perfil por medio del Id de su cuenta.
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta.
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



    /// <summary>
    /// Obtiene una lista de usuarios según los Ids de las cuentas.
    /// </summary>
    /// <param name="ids">Ids de las cuentas</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<ProfileModel>> ReadByAccounts(IEnumerable<int> ids, Conexión context)
    {

        // Ejecución
        try
        {
            // Consulta.
            var profiles = await (from P in context.DataBase.Profiles
                                 where ids.Contains(P.AccountID)
                                 select P).ToListAsync();

            if (profiles == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, profiles ?? []);
        }
        catch
        {
        }
        return new();
    }



    /// <summary>
    /// Establecer la ultima conexión.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="time">Hora de conexión</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> SetLastConnection(int id, DateTime time, Conexión context)
    {


        // Ejecución
        try
        {

            // Consulta.
            var profile = await (from P in context.DataBase.Profiles
                                 where P.AccountID == id
                                 select P).FirstOrDefaultAsync();

            if (profile == null)
                return new(Responses.NotExistProfile);

            profile.LastConnection = time;
            context.DataBase.SaveChanges();

            return new(Responses.Success);
        }
        catch
        {
        }
        return new();
    }



    /// <summary>
    /// Obtener la ultima conexión.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<DateTime>> GetLastConnection(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta.
            var lastConnection = await (from P in context.DataBase.Profiles
                                 where P.AccountID == id
                                 select P.LastConnection).FirstOrDefaultAsync();

            // Respuesta.
            return new(Responses.Success, lastConnection);
        }
        catch
        {
        }
        return new();
    }



}