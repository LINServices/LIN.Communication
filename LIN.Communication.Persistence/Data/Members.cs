namespace LIN.Communication.Persistence.Data;

public class Members(Context context)
{

    /// <summary>
    /// Insertar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    public async Task<ResponseBase> Create(int id, int profile)
    {

        // Ejecución
        try
        {
            // Consulta
            var group = await (from conversation in context.Conversations
                               where conversation.ID == id
                               select conversation).FirstOrDefaultAsync();

            // No existe la conversación.
            if (group is null)
                return new(Responses.NotRows);

            // Validar el integrante ya existe.
            var exist = await (from conversation in context.Conversations
                               where conversation.ID == id
                               join MM in context.Members
                               on conversation.ID equals MM.Conversation.ID
                               where MM.Profile.ID == profile
                               select MM).AnyAsync();

            // Si el integrante ya existe.
            if (exist)
                return new(Responses.ResourceExist);

            // Si es una conversación personal, se convierte en grupo.
            if (group.Type == ConversationsTypes.Personal)
            {
                group.Type = ConversationsTypes.Group;
                group.Name = "Grupo";
            }

            // Perfil ya existe.
            var profileModel = new ProfileModel()
            {
                ID = profile
            };

            context.Attach(profileModel);

            var member = new MemberChatModel()
            {
                Conversation = group,
                Profile = profileModel,
                Rol = MemberRoles.None
            };

            group.Members.Add(member);

            context.SaveChanges();

            return new(Responses.Success);
        }
        catch
        {
        }
        return new();
    }


    /// <summary>
    /// Obtiene los miembros asociadas a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.Members
                                where M.Conversation.ID == id
                                select new MemberChatModel
                                {
                                    Profile = new()
                                    {
                                        Alias = M.Profile.Alias,
                                        ID = M.Profile.ID,
                                        IdentityId = M.Profile.IdentityId
                                    },
                                    Rol = M.Rol,
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }


    /// <summary>
    /// Eliminar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    public async Task<ResponseBase> Remove(int id, int profile)
    {

        // Ejecución
        try
        {

            // Consulta.
            await (from M in context.Members
                   where M.Profile.ID == profile
                   && M.Conversation.ID == id
                   select M).ExecuteDeleteAsync();

            return new(Responses.Success);
        }
        catch
        {
        }
        return new();
    }

}