﻿namespace LIN.Communication.Data;


public class Profiles
{



    #region Abstracciones


    /// <summary>
    /// Crea un perfil.
    /// </summary>
    /// <param name="data">Modelo.</param>
    public async static Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene un perfil
    /// </summary>
    /// <param name="id">ID del perfil</param>
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
    /// Obtiene un perfil por medio del ID de su cuenta.
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadOneResponse<ProfileModel>> ReadByAccount(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadByAccount(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    public async static Task<ReadAllResponse<ProfileModel>> ReadByAccounts(IEnumerable<int> ids)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadByAccounts(ids, context);

        context.CloseActions(connectionKey);

        return response;

    }




    public async static Task<ReadOneResponse<DateTime>> GetLastConnection(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await GetLastConnection(id, context);

        context.CloseActions(connectionKey);

        return response;

    }


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
        // ID
        data.Profile.ID = 0;
        data.Profile.Friends = new();

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
    /// Obtiene un perfil
    /// </summary>
    /// <param name="id">ID del perfil</param>
    /// <param name="context">Contexto de conexión.</param>
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



    /// <summary>
    /// Obtiene un perfil por medio del ID de su cuenta.
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión.</param>
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



   
    public async static Task<ReadAllResponse<ProfileModel>> ReadByAccounts(IEnumerable<int> ids, Conexión context)
    {


        // Ejecución
        try
        {

            var profile = await (from P in context.DataBase.Profiles
                                 where ids.Contains(P.AccountID)
                                 select P).ToListAsync();

            if (profile == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, profile ?? new());
        }
        catch
        {
        }
        return new();
    }




    public async static Task<ResponseBase> SetLastConnection(int id, DateTime time, Conexión context)
    {


        // Ejecución
        try
        {

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


    public async static Task<ReadOneResponse<DateTime>> GetLastConnection(int id, Conexión context)
    {


        // Ejecución
        try
        {

            var profile = await (from P in context.DataBase.Profiles
                                 where P.AccountID == id
                                 select P.LastConnection).FirstOrDefaultAsync();


            return new(Responses.Success, profile);
        }
        catch
        {
        }
        return new();
    }



}