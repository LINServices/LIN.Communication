﻿namespace LIN.Communication.Data;


public class Conversations
{


    #region Abstracciones



    /// <summary>
    /// Crea una conversación.
    /// </summary>
    /// <param name="data">Modelo</param>
    public async static Task<CreateResponse> Create(ConversationModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async static Task<ReadOneResponse<MemberChatModel>> ReadOne(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadOne(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Agregar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    public async static Task<ResponseBase> InsertMember(int id, int profile)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await InsertMember(id, profile, context);

        context.CloseActions(connectionKey);

        return response;

    }



    /// <summary>
    /// Obtiene los miembros asociadas a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadMembers(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadMembers(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    #endregion



    /// <summary>
    /// Crea una conversación (Grupo).
    /// </summary>
    /// <param name="data">Modelo</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<CreateResponse> Create(ConversationModel data, Conexión context)
    {
        // Id
        data.ID = 0;

        // Ejecución
        try
        {

            foreach (var user in data.Members)
                context.DataBase.Attach(user.Profile);

            var res = context.DataBase.Conversaciones.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch (Exception)
        {
        }
        return new();
    }



    /// <summary>
    /// Obtiene las conversaciones asociadas a un perfil.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members.Include(x => x.Conversation.Members)
                                where M.Profile.ID == id
                                && M.Conversation.Visibility == ConversationVisibility.@public
                                select new MemberChatModel
                                {
                                    Conversation = new ConversationModel
                                    {
                                        ID = M.Conversation.ID,
                                        Name = M.Conversation.Name,
                                        Type = M.Conversation.Type,
                                        Visibility = M.Conversation.Visibility,
                                        Members = (M.Conversation.Type == ConversationsTypes.Personal)
                                                    ? (from member in M.Conversation.Members
                                                       select new MemberChatModel
                                                       {
                                                           ID = member.ID,
                                                           Profile = new()
                                                           {
                                                               ID = member.Profile.ID,
                                                               AccountID = member.Profile.AccountID,
                                                           }
                                                       }).ToList() : new List<MemberChatModel>()
                                    },
                                    Rol = M.Rol
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }



    /// <summary>
    /// Obtiene una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ReadOneResponse<MemberChatModel>> ReadOne(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members.Include(x => x.Conversation.Members)
                                where M.ID == id
                                && M.Conversation.Visibility == ConversationVisibility.@public
                                select new MemberChatModel
                                {
                                    Conversation = new ConversationModel
                                    {
                                        ID = M.Conversation.ID,
                                        Name = M.Conversation.Name,
                                        Type = M.Conversation.Type,
                                        Visibility = M.Conversation.Visibility,
                                        Members = (M.Conversation.Type == ConversationsTypes.Personal)
                                                    ? (from member in M.Conversation.Members
                                                       select new MemberChatModel
                                                       {
                                                           ID = member.ID,
                                                           Rol = member.Rol,
                                                           Profile = new()
                                                           {
                                                               ID = member.Profile.ID,
                                                               AccountID = member.Profile.AccountID,
                                                           }
                                                       }).ToList() : new List<MemberChatModel>()
                                    },
                                    Rol = M.Rol
                                }).FirstOrDefaultAsync();


            if (groups == null)
                return new(Responses.NotRows);

            return new(Responses.Success, groups);
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
    public async static Task<ReadAllResponse<MemberChatModel>> ReadMembers(int id, Conexión context)
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
    /// Insertar un miembro a una conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="context">Contexto de conexión.</param>
    public async static Task<ResponseBase> InsertMember(int id, int profile, Conexión context)
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


}