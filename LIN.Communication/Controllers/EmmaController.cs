using LIN.Types.Emma.Models;

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
    [Experimental("Este método necesita la función de Threads")]
    public async Task<HttpReadOneResponse<ResponseIAModel>> Assistant([FromHeader] string token, [FromBody] string consult)
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

        // Cargar el modelo
        modelIA.Load(IA.IAConsts.Base);
        modelIA.Load(IA.IAConsts.Personalidad);
        modelIA.Load(IA.IAConsts.ComandosBase);
        modelIA.Load(IA.IAConsts.Comandos);

        // Recomendaciones del contexto
        modelIA.Load($"""
            Estas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
            Estos son los nombres de los chats que tiene el usuario: {session.StringOfConversations()}
            Recuerda que si el usuario quiere mandar un mensaje a un usuario/grupo/team/conversación etc, primero busca en su lista de nombres de chats
            """);

        // Contexto del usuario
        modelIA.Load($"""
            El alias del usuario es '{alias}'.
            El usuario tiene {session.Devices.Count} sesiones (dispositivos) conectados actualmente a LIN Allo.
            """);

        // Respuesta
        var response = await modelIA.Reply(consult);

        // Respuesta
        return new ReadOneResponse<ResponseIAModel>()
        {
            Model = response,
            Response = response.IsSuccess ? Responses.Success : Responses.Undefined
        };

    }



}