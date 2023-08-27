﻿namespace LIN.Communication.Data;


public class Conversations
{



    #region Abstracciones


    public async static Task<CreateResponse> Create(ConversationModel data)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await Create(data, context);

        context.CloseActions(connectionKey);

        return response;

    }



    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadAll(id, context);

        context.CloseActions(connectionKey);

        return response;

    }



    public async static Task<ReadAllResponse<MessageModel>> ReadMessages(int id)
    {

        // Contexto
        (Conexión context, string connectionKey) = Conexión.GetOneConnection();

        // respuesta
        var response = await ReadMessages(id, context);

        context.CloseActions(connectionKey);

        return response;

    }






    #endregion





    public async static Task<CreateResponse> Create(ConversationModel data, Conexión context)
    {
        // ID
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
        catch
        {
        }
        return new();
    }



    public async static Task<CreateResponse> Create(MessageModel data, Conexión context)
    {
        // ID
        data.ID = 0;

        // Ejecución
        try
        {

            context.DataBase.Attach(data.Conversacion);
            context.DataBase.Attach(data.Remitente);

            var res = context.DataBase.Mensajes.Add(data);
            await context.DataBase.SaveChangesAsync();
            return new(Responses.Success, data.ID);
        }
        catch
        {
        }
        return new();
    }



    public async static Task<ReadAllResponse<MemberChatModel>> ReadAll(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Members
                                where M.Profile.ID == id
                                select new MemberChatModel
                                {
                                    Conversation = M.Conversation,
                                    Rol = M.Rol,
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }



    public async static Task<ReadAllResponse<MessageModel>> ReadMessages(int id, Conexión context)
    {

        // Ejecución
        try
        {

            // Consulta
            var groups = await (from M in context.DataBase.Mensajes
                                where M.Conversacion.ID == id
                                select new MessageModel
                                {
                                    Contenido = M.Contenido,
                                    Conversacion = M.Conversacion,
                                    ID = M.ID,
                                    Remitente = M.Remitente,
                                    Time = M.Time
                                }).ToListAsync();

            return new(Responses.Success, groups);
        }
        catch
        {
        }
        return new();
    }



}