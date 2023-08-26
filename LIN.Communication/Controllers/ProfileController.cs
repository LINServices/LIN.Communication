namespace LIN.Communication.Controllers;


[Route("profile")]
public class ProfileController : ControllerBase
{


    /// <summary>
    /// Crea un nuevo contacto
    /// </summary>
    /// <param name="modelo">Modelo del contacto</param>
    [HttpPost("create")]
    public async Task<HttpCreateResponse> Create([FromBody] ContactDataModel modelo)
    {

        // Comprobación de campos
        if (modelo.Name.Length <= 0 || modelo.ProfileID <= 0)
            return new(Responses.InvalidParam);

        string @default = "Sin definir";

        modelo.Direction ??= @default;
        modelo.Phone ??= @default;
        modelo.Mail ??= @default;
        modelo.State = ContactStatus.Normal;

        // Obtiene el resultado
        var response = await Data.Contacts.Create(modelo);

        // Retorna el resultado
        return response ?? new();

    }



    /// <summary>
    /// Obtiene un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    [HttpGet("read")]
    public async Task<HttpReadOneResponse<ContactDataModel>> Read([FromHeader] int id)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Contacts.Read(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Obtiene los contactos asociados a una cuenta
    /// </summary>
    /// <param name="id">ID de la cuenta</param>
    [HttpGet("read/all")]
    public async Task<HttpReadAllResponse<ContactDataModel>> ReadAll([FromHeader] int id)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Contacts.ReadAll(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Actualiza la información de un contacto
    /// </summary>
    /// <param name="modelo">Modelo del contacto</param>
    [HttpPatch("update")]
    public async Task<HttpResponseBase> Update([FromBody] ContactDataModel modelo)
    {

        // Comprobación de campos
        if (modelo.Name.Length <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Contacts.Update(modelo);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Cuenta cuantos contactos tiene una cuenta
    /// </summary>
    /// <param name="token">Token de acceso</param>
    [HttpGet("count")]
    public async Task<HttpReadOneResponse<int>> Count([FromHeader] string token)
    {

        var (isValid, _, id) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.Unauthorized);


        // Obtiene el usuario
        var result = await Data.Contacts.Count(id);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Elimina un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    /// <param name="token">Token de acceso</param>
    [HttpDelete("delete")]
    public async Task<HttpResponseBase> Delete([FromHeader] int id, [FromHeader] string token)
    {

        var (isValid, _, _) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.InvalidParam);

        // Comprobación de campos
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Contacts.UpdateStatus(id, ContactStatus.Deleted);

        // Retorna el resultado
        return result ?? new();

    }



    /// <summary>
    /// Envía a la papelera un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    /// <param name="token">Token de acceso</param>
    [HttpDelete("trash")]
    public async Task<HttpResponseBase> ToTrash([FromHeader] int id, [FromHeader] string token)
    {

        var (isValid, _, _) = Jwt.Validate(token);

        if (!isValid)
            return new(Responses.InvalidParam);

        // Comprobación de campos
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Contacts.UpdateStatus(id, ContactStatus.OnTrash);

        // Retorna el resultado
        return result ?? new();

    }



}