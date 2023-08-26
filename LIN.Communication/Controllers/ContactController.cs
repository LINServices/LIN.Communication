namespace LIN.Communication.Controllers;


[Route("conversations")]
public class ContactController : ControllerBase
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
        var response = await Data.Contacts.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }





    /// <summary>
    /// Obtiene los contactos asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<ConversaciónModel>> ReadAll([FromHeader] string token)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Contacts.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }

}