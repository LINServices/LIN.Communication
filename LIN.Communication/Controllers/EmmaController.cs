namespace LIN.Communication.Controllers;


[Route("Emma")]
public class EmmaController : ControllerBase
{


    /// <summary>
    /// Consulta para LIN Allo Emma.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="consult">Consulta.</param>
    [HttpPost]
    public async Task<HttpReadOneResponse<ResponseIAModel>> Assistant([FromHeader] string token, [FromHeader] string? thread, [FromBody] string consult)
    {

        // Info del token.
        var (isValid, profileID, _, alias) = Jwt.Validate(token);

        // Token es invalido.
        if (!isValid)
            return new ReadOneResponse<ResponseIAModel>()
            {
                Response = Responses.Unauthorized
            };

        // Obtiene la sesión
        var session = Mems.Sessions[profileID] ?? new();

        // Modelo de Emma.
        var modelIA = new Access.OpenIA.IAModelBuilder(Configuration.GetConfiguration("openIa:key"));


        // Valida el hilo.
        var threadModel = ThreadsEmma.Threads.Where(x => x.Key == (thread ?? "")).FirstOrDefault();

        // Valida el hilo valido.
        if (threadModel.Key == null || threadModel.Value == null)
        {
            // Nuevo hilo.
            threadModel = new(Guid.NewGuid().ToString(), []);
            ThreadsEmma.Threads.Add(threadModel.Key, threadModel.Value);

            // Cargar el hilo.
            threadModel.Value.Add(new(IA.IAConsts.Base, Roles.System));
            threadModel.Value.Add(new(IA.IAConsts.Personalidad, Roles.System));
            threadModel.Value.Add(new(IA.IAConsts.ComandosBase, Roles.System));
            threadModel.Value.Add(new(IA.IAConsts.Comandos, Roles.System));
            threadModel.Value.Add(new($"""
            Estas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
            Estos son los nombres de los chats que tiene el usuario: {session.StringOfConversations()}
            Recuerda que si el usuario quiere mandar un mensaje a un usuario/grupo/team/conversación etc, primero busca en su lista de nombres de chats
            """, Roles.System));

            // Contexto del usuario
            threadModel.Value.Add(new($"""
            El alias del usuario es '{alias}'.
            El usuario tiene {session.Devices.Count} sesiones (dispositivos) conectados actualmente a LIN Allo.
            """, Roles.System));

        }

        // Consulta del usuario.
        threadModel.Value.Add(new(consult, Roles.User));

        // Armar el modelo IA.
        foreach (var x in threadModel.Value)
        {
            if (x.Rol == Roles.System)
                modelIA.Load(x.Content);

            if (x.Rol == Roles.User)
                modelIA.LoadFromUser(x.Content);

            if (x.Rol == Roles.Emma)
                modelIA.LoadFromEmma(x.Content);
        }

        // Respuesta
        var response = await modelIA.Reply();

        if (response.IsSuccess)
            threadModel.Value.Add(new(response.Content, Roles.Emma));


        // Respuesta
        return new ReadOneResponse<ResponseIAModel>()
        {
            Model = response,
            Response = response.IsSuccess ? Responses.Success : Responses.Undefined,
            Message = threadModel.Key
        };

    }



}