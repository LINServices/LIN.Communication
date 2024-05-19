namespace LIN.Communication.Data;


public partial class Members
{


    /// <summary>
    /// Insertar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> Create(int id, int profile, Conexión context)
    {

        // Ejecución
        try
        {
// Consulta
var group = await (from M in context.DataBase.Conversaciones
       where M.ID == id
       select M).FirstOrDefaultAsync();

if (group == null)
{
    return new(Responses.NotRows);
}


var exist = await (from M in context.DataBase.Conversaciones
       where M.ID == id
       join MM in context.DataBase.Members
       on M.ID equals MM.Conversation.ID
       where MM.Profile.ID == profile
       select MM).AnyAsync();

if (exist)
{
    return new(Responses.Success);
}

if (group.Type == ConversationsTypes.Personal)
{
    group.Type = ConversationsTypes.Group;
    group.Name = "Grupo";
}


var profileModel = new ProfileModel()
{
    ID = profile
};

context.DataBase.Attach(profileModel);

var member = new MemberChatModel()
{
    Conversation = group,
    Profile = profileModel,
    Rol = Types.Communication.Enumerations.MemberRoles.None
};

group.Members.Add(member);

context.DataBase.SaveChanges();

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
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

// Consulta
var groups = await (from M in context.DataBase.Members
        where M.Conversation.ID == id
        select new MemberChatModel
        {
Profile = new()
{
    Alias = M.Profile.Alias,
    ID = M.Profile.ID,
    AccountID = M.Profile.AccountID
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
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> Remove(int id, int profile, Conexión context)
    {

        // Ejecución
        try
        {

// Consulta.
var deleted = await (from M in context.DataBase.Members
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