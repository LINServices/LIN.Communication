using System.Reflection.Metadata.Ecma335;

namespace LIN.Communication.Controllers;


[Route("conversations")]
public class ConversationController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo contacto
    /// </summary>
    /// <param name="modelo">Modelo del contacto</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ConversaciónModel modelo)
    {

        string @default = "Sin definir";

        // Obtiene el resultado
        var response = await Data.Conversations.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }




    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<ConversaciónModel>> ReadAll([FromHeader] string token)
    {

        // Comprobaciones
        var (isValid, profileID, _, _) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.Unauthorized);

        // Obtiene el usuario
        var result = await Data.Conversations.ReadAll(profileID);

        // Retorna el resultado
        return result ?? new();

    }

}