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



    /// <summary>
    /// Inicia una sesión de usuario
    /// </summary>
    /// <param name="user">Usuario único</param>
    /// <param name="password">Contraseña del usuario</param>
    [HttpGet("login")]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> Login([FromQuery] string user, [FromQuery] string password)
    {

        // Comprobación
        if (!user.Any() || !password.Any())
            return new(Responses.InvalidParam);

        // Respuesta de autenticación
        var authResponse = await LIN.Access.Auth.Controllers.Authentication.Login(user, password);

        // Autenticación errónea
        if (authResponse.Response != Responses.Success)
        {
            return new ReadOneResponse<AuthModel<ProfileModel>>
            {
                Message = "Autenticación fallida",
                Response = authResponse.Response
            };
        }

        // Obtiene el perfil
        var profile = await Data.Profiles.ReadByAccount(authResponse.Model.ID);

        switch (profile.Response)
        {
            case Responses.Success:
                break;
            case Responses.NotExistProfile:
                {
                    var res = await Data.Profiles.Create(new()
                    {
                        AccountID = authResponse.Model,
                        Profile = new()
                        {
                            AccountID = authResponse.Model.ID,
                            Creación = DateTime.Now
                        }
                    });

                    if (res.Response != Responses.Success)
                    {
                        return new ReadOneResponse<AuthModel<ProfileModel>>
                        {
                            Response = Responses.UnavailableService,
                            Message = "Un error grave ocurrió"
                        };
                    }

                    profile = res;
                    break;
                }
            default:
                return new ReadOneResponse<AuthModel<ProfileModel>>
                {
                    Response = Responses.UnavailableService,
                    Message = "Un error grave ocurrió"
                };
        }


        // Genera el token
        var token = Jwt.Generate(profile.Model);

        return new ReadOneResponse<AuthModel<ProfileModel>>
        {
            Response = Responses.Success,
            Message = "Success",
            Model = new()
            {
                Account = authResponse.Model,
                LINAuthToken = authResponse.Token,
                Profile = profile.Model
            },
            Token = token
        };

    }



}