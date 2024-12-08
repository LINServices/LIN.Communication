namespace LIN.Communication.Persistence.Data;

public class Profiles(Context context)
{

    /// <summary>
    /// Crea un perfil.
    /// </summary>
    /// <param name="data">Modelo.</param>
    public async Task<ReadOneResponse<ProfileModel>> Create(AuthModel<ProfileModel> data)
    {
        // Id
        data.Profile.Id = 0;
        data.Profile.Friends = [];

        // Ejecución
        try
        {
            var res = context.Profiles.Add(data.Profile);
            await context.SaveChangesAsync();
            return new(Responses.Success, data.Profile);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene un perfil.
    /// </summary>
    /// <param name="id">Id del perfil</param>
    public async Task<ReadOneResponse<ProfileModel>> Read(int id)
    {

        // Ejecución
        try
        {

            // Consulta.
            var profile = await (from P in context.Profiles
                                 where P.Id == id
                                 select P).FirstOrDefaultAsync();

            return new(Responses.Success, profile ?? new());
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene un perfil por medio del Id de su cuenta.
    /// </summary>
    /// <param name="id">Id de la cuenta</param>
    public async Task<ReadOneResponse<ProfileModel>> ReadByIdentity(int id)
    {

        // Ejecución
        try
        {

            // Consulta.
            var profile = await (from P in context.Profiles
                                 where P.IdentityId == id
                                 select P).FirstOrDefaultAsync();

            if (profile == null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, profile ?? new());
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene una lista de usuarios según los Ids de las cuentas.
    /// </summary>
    /// <param name="ids">Ids de las cuentas</param>
    public async Task<ReadAllResponse<ProfileModel>> ReadByIdentities(IEnumerable<int> ids)
    {

        // Ejecución
        try
        {
            // Consulta.
            var profiles = await (from P in context.Profiles
                                  where ids.Contains(P.IdentityId)
                                  select P).ToListAsync();

            if (profiles is null)
                return new(Responses.NotExistProfile);

            return new(Responses.Success, profiles ?? []);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Establecer la ultima conexión.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="time">Hora de conexión</param>
    public async Task<ResponseBase> SetLastConnection(int id, DateTime time)
    {

        // Ejecución
        try
        {

            // Consulta.
            var profile = await (from P in context.Profiles
                                 where P.IdentityId == id
                                 select P).FirstOrDefaultAsync();

            if (profile is null)
                return new(Responses.NotExistProfile);

            profile.LastConnection = time;
            context.SaveChanges();

            return new(Responses.Success);
        }
        catch (Exception)
        {
        }
        return new();
    }


    /// <summary>
    /// Obtener la ultima conexión.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async Task<ReadOneResponse<DateTime>> GetLastConnection(int id)
    {

        // Ejecución
        try
        {

            // Consulta.
            var lastConnection = await (from P in context.Profiles
                                        where P.IdentityId == id
                                        select P.LastConnection).FirstOrDefaultAsync();

            // Respuesta.
            return new(Responses.Success, lastConnection);
        }
        catch (Exception)
        {
        }
        return new();
    }

}