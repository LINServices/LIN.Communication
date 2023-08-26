namespace LIN.Communication.Controllers;


[Route("profile")]
public class ProfileController : ControllerBase
{


    /// <summary>
    /// Obtiene un contacto
    /// </summary>
    /// <param name="id">ID del contacto</param>
    [HttpGet("read")]
    public async Task<HttpReadOneResponse<ProfileModel>> Read([FromHeader] int id)
    {

        // Comprobaciones
        if (id <= 0)
            return new(Responses.InvalidParam);

        // Obtiene el usuario
        var result = await Data.Profiles.Read(id);

        // Retorna el resultado
        return result ?? new();

    }



}