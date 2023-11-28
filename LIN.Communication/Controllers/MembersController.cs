namespace LIN.Communication.Controllers;


[Route("conversations")]
public class MembersController : ControllerBase
{


    /// <summary>
    /// Validación si un usuario esta online.
    /// </summary>
    /// <param name="id">Id del usuario.</param>
    [HttpGet("isOnline")]
    public async Task<HttpReadOneResponse<IsOnlineResult>> ReadOnline([FromQuery] int id)
    {

        // Obtiene el perfil
        var profile = Mems.Sessions[id];

        // Perfil no existe.
        if (profile == null)
            return new()
            {
                Message = "Sesión no encontrada",
                Response = Responses.NotRows
            };
        
        return new ReadOneResponse<IsOnlineResult>()
        {
            Response = Responses.Success,
            Model = new()
            {
                ID = id,
                IsOnline = profile?.Devices.Count != 0,
                LastTime = profile?.LastTime ?? (await Data.Profiles.GetLastConnection(id)).Model,
            }
        };

    }



    /// <summary>
    /// Obtiene los miembros de una conversación.
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("{id:int}/members")]
    public async Task<HttpReadAllResponse<MemberChatModel>> ReadAll([FromRoute] int id, [FromHeader] string token)
    {

        // Obtiene la info del token
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        // Token es invalido
        if (!isValid)
            return new ReadAllResponse<MemberChatModel>()
            {
                Message = "El token es invalido.",
                Response = Responses.Unauthorized
            };

        // Busca el acceso
        var iam = await Services.Iam.Conversation.Validate(profileID, id);

        if (iam == Types.Enumerations.IamLevels.NotAccess)
            return new ReadAllResponse<MemberChatModel>
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };

        // Obtiene el usuario
        var result = await Data.Conversations.ReadMembers(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene los miembros de una conversación con info del usuario.
    /// </summary>
    /// <param name="id">ID de la conversación.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("{id:int}/members/info")]
    public async Task<HttpReadAllResponse<SessionModel<MemberChatModel>>> ReadAllInfo([FromRoute] int id, [FromHeader] string token, [FromHeader] string tokenAuth)
    {

        // Información del token.
        var (isValid, profile, _, _) = Jwt.Validate(token);

        // Si el token es invalido.
        if (!isValid)
            return new()
            {
                Message = "Token invalido.",
                Response = Responses.Unauthorized
            };

        // Validación Iam.
        var iam = await Services.Iam.Conversation.Validate(profile, id);

        // Valida el acceso Iam.
        if (iam == Types.Enumerations.IamLevels.NotAccess)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a esta conversación."
            };


        // Obtiene los miembros.
        var members = await Data.Conversations.ReadMembers(id);

        // Obtiene los Id de las cuentas.
        var accountsId = members.Models.Select(member => member.Profile.AccountID).ToList();

        // Información de las cuentas.
        var accounts = await Access.Auth.Controllers.Account.Read(accountsId, tokenAuth);

        // Armar los modelos.
        var response = (from member in members.Models
                        join account in accounts.Models
                        on member.Profile.AccountID equals account.ID
                        select new SessionModel<MemberChatModel>
                        {
                            Account = account,
                            Profile = new()
                            {
                                Rol = member.Rol,
                                Profile = new()
                                {
                                    ID = member.Profile.ID,
                                    Alias = member.Profile.Alias,
                                    LastConnection = member.Profile.LastConnection,
                                }
                            }
                        }).ToList();

        // Retorna el resultado
        return new ReadAllResponse<SessionModel<MemberChatModel>>
        {
            Models = response,
            Response = Responses.Success
        };

    }



    /// <summary>
    /// Agrega un miembro a una conversación.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("{id:int}/members/add")]
    public async Task<HttpResponseBase> AddTo([FromRoute] int id, [FromQuery] int profileId, [FromHeader] string token)
    {

        // Información del token.
        var (isValid, meId, _, _) = Jwt.Validate(token);

        // Invalido.
        if (!isValid)
            return new()
            {
                Message = "Token invalido",
                Response = Responses.Unauthorized
            };

        // Validación Iam.
        var iam = await Services.Iam.Conversation.Validate(meId, id);

        // Valida el acceso Iam.
        if (iam != IamLevels.Privileged)
            return new()
            {
                Response = Responses.Unauthorized,
                Message = "No tienes permisos para modificar a esta conversación."
            };

        // Insertar el integrante.
        var response = await Data.Conversations.InsertMember(id, profileId);

        return new()
        {
            Message = response.Response == Responses.Success ? "Correcto" : "No se pudo insertar el integrante.",
            Response = response.Response
        };
    }



    /// <summary>
    /// Encuentra o crea una conversación personal.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="friendId">Id del otro usuario.</param>
    [HttpPost("find")]
    public async Task<HttpCreateResponse> Find([FromHeader] string token, [FromHeader] int friendId)
    {

        // Información del token.
        var (isValid, profile, _, _) = Jwt.Validate(token);

        // Si el token es invalido.
        if (!isValid)
            return new()
            {
                Message = "Token invalido.",
                Response = Responses.Unauthorized
            };


        // Contexto de conexión.
        var (context, contextKey) = Conexión.GetOneConnection();

        // Consulta.
        var conversation = await (from u in context.DataBase.Conversaciones
                       where u.Type == ConversationsTypes.Personal
                       && u.Members.Count == 2
                       && u.Members.Where(t => t.Profile.ID == friendId).Any()
                       && u.Members.Where(t => t.Profile.ID == profile).Any()
                       select u).FirstOrDefaultAsync();

        
        // Si ya existe.
        if (conversation != null)
        {
            context.CloseActions(contextKey);
            return new CreateResponse()
            {
                Response = Responses.Success,
                LastID = conversation.ID,
                Message ="Se encontró."
            };
        }


        // Crear el chat
        var response = await Data.Conversations.Create(new()
        {
            ID = 0,
            Name = "Chat Personal",
            Type = ConversationsTypes.Personal,
            Visibility = ConversationVisibility.@public,
            Members = [
                     new MemberChatModel()
                     {
                         ID = 0,
                         Profile = new()
                         {
                             ID = profile
                         },
                         Rol = MemberRoles.Admin
                     },

                new MemberChatModel()
                {
                    ID = 0,
                    Profile = new()
                    {
                        ID = friendId
                    },
                    Rol = MemberRoles.Admin
                }
                 ]
        }, context);

        // Cierra la conexión.
        context.CloseActions(contextKey);

        // Retorna el resultado
        return response;

    }



}