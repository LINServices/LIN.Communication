namespace LIN.Communication.Data;


public class Contacts
{



    #region Abstracciones


    /// <summary>
    /// Crea un nuevo contacto
    /// </summary>
    /// <param name="data">Modelo del contacto</param>
    public async static Task<CreateResponse> Create(ContactDataModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    public async static Task<ReadOneResponse<ContactDataModel>> Read(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Read(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene la lista de contactos asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadAllResponse<ContactDataModel>> ReadAll(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await ReadAll(id, context);
        context.CloseActions(connectionKey);
        return response;

    }



    /// <summary>
    /// Actualiza el estado de un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    public async static Task<ResponseBase> UpdateStatus(int id, ContactStatus status)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await UpdateStatus(id, status, context);
        context.CloseActions(connectionKey);
        return response;

    }



    /// <summary>
    /// Actualiza la información de un contacto
    /// </summary>
    /// <param name="modelo">Nueva información</param>
    public async static Task<ResponseBase> Update(ContactDataModel modelo)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var res = await Update(modelo, context);

        context.CloseActions(connectionKey);

        return res;

    }



    /// <summary>
    /// Obtiene la cantidad de contactos asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    public async static Task<ReadOneResponse<int>> Count(int id)
    {

        // Obtiene la conexión
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        var response = await Count(id, context);
        context.CloseActions(connectionKey);
        return response;

    }



    #endregion



    /// <summary>
    /// Crea un nuevo contacto
    /// </summary>
    /// <param name="data">Modelo del contacto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<CreateResponse> Create(ContactDataModel data, Conexión context)
    {
        // ID
        data.ID = 0;

        // Ejecución
        try
        {
            var res = context.DataBase.Contactos.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);

        }
        return new();
    }



    /// <summary>
    /// Obtiene un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<ContactDataModel>> Read(int id, Conexión context)
    {

        // Ejecución
        try
        {
            var res = await context.DataBase.Contactos.FirstOrDefaultAsync(T => T.ID == id);

            // Si no existe el modelo
            if (res == null)
            {
                return new();
            }


            if (res.State == ContactStatus.Deleted)
            {
                res.Picture = Array.Empty<byte>();
                res.Phone = string.Empty;
                res.Mail = string.Empty;
                res.Direction = string.Empty;
                res.Name = "Contacto Eliminado";
            }

            return new(Responses.Success, res);
        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la lista de contactos asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadAllResponse<ContactDataModel>> ReadAll(int id, Conexión context)
    {
        // Ejecución
        try
        {
            var res = await context.DataBase.Contactos
                .Where(T => T.ProfileID == id && T.State != ContactStatus.Deleted).ToListAsync();

            // Si no existe el modelo
            res ??= new();

            return new(Responses.Success, res);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError(ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Cambia el estado un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> UpdateStatus(int id, ContactStatus status, Conexión context)
    {

        // Ejecución
        try
        {
            var user = await context.DataBase.Contactos.FindAsync(id);

            if (user != null)
            {
                user.State = status;
                context.DataBase.SaveChanges();
                return new(Responses.Success);
            }

            return new(Responses.NotRows);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError("Grave-- " + ex.Message);
        }
        return new();
    }



    /// <summary>
    /// Actualiza la información de un contacto
    /// </summary>
    /// <param name="modelo">Modelo del contacto</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ResponseBase> Update(ContactDataModel modelo, Conexión context)
    {

        // Ejecución
        try
        {
            var contacto = await context.DataBase.Contactos.FindAsync(modelo.ID);

            if (contacto == null)
            {
                return new(Responses.NotRows);
            }

            string @default = "Sin definir";

            contacto.Name = modelo.Name;
            contacto.Phone = modelo.Phone ?? @default;
            contacto.Direction = modelo.Direction ?? @default;
            contacto.Picture = modelo.Picture;
            contacto.Mail = modelo.Mail ?? @default;

            context.DataBase.SaveChanges();

            return new(Responses.Success);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError("Grave-- " + ex.Message);
        }

        return new();
    }



    /// <summary>
    /// Obtiene la cantidad de contactos
    /// </summary>
    /// <param name="account">ID de la cuenta</param>
    /// <param name="context">Contexto de conexión</param>
    public async static Task<ReadOneResponse<int>> Count(int account, Conexión context)
    {

        // Ejecución
        try
        {

            var count = await (from C in context.DataBase.Contactos
                               where C.ProfileID == account
                               where C.State == ContactStatus.Normal
                               select C).CountAsync();


            return new(Responses.Success, count);

        }
        catch (Exception ex)
        {
            ServerLogger.LogError("Grave-- " + ex.Message);
        }

        return new();
    }



}