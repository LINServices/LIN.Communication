namespace LIN.Communication.Controllers;

[Route("[controller]")]
public class ProfileController(Persistence.Data.Profiles profilesData) : ControllerBase
{

    /// <summary>
    /// Inicia una sesión con credenciales.
    /// </summary>
    /// <param name="user">Usuario único</param>
    /// <param name="password">Contraseña del usuario</param>
    [HttpGet("login")]
    [RateLimit(requestLimit: 5, timeWindowSeconds: 60, blockDurationSeconds: 120)]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> Login([FromQuery] string user, [FromQuery] string password)
    {

        // Validación de datos.
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            return new(Responses.InvalidParam);

        // Respuesta de autenticación
        var authResponse = await Access.Auth.Controllers.Authentication.Login(user, password);

        // Autenticación errónea
        if (authResponse.Response != Responses.Success)
        {
            return new ReadOneResponse<AuthModel<ProfileModel>>
            {
                Message = "Autenticación fallida",
                Response = authResponse.Response
            };
        }

        // Obtiene el perfil según el id de la identidad.
        var profile = await profilesData.ReadByIdentity(authResponse.Model.IdentityId);

        switch (profile.Response)
        {
            case Responses.Success:
                break;

            case Responses.NotExistProfile:
                {
                    // Crear el perfil en caso de no existir.
                    var res = await profilesData.Create(new()
                    {
                        Account = authResponse.Model,
                        Profile = new()
                        {
                            IdentityId = authResponse.Model.IdentityId,
                            Alias = authResponse.Model.Name
                        }
                    });

                    // Si hubo un error al crear el perfil.
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

        // Genera el token de acceso.
        var token = Jwt.Generate(profile.Model);

        return new ReadOneResponse<AuthModel<ProfileModel>>
        {
            Response = Responses.Success,
            Message = "Success",
            Model = new()
            {
                Account = authResponse.Model,
                TokenCollection = new()
                {
                    { "identity", authResponse.Token}
                },
                Profile = profile.Model
            },
            Token = token
        };

    }


    /// <summary>
    /// Iniciar sesión con token.
    /// </summary>
    /// <param name="token">Token</param>
    [HttpGet("login/token")]
    [RateLimit(requestLimit: 5, timeWindowSeconds: 60, blockDurationSeconds: 120)]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> LoginToken([FromQuery] string token)
    {

        // Login en LIN Server
        var response = await Access.Auth.Controllers.Authentication.Login(token);

        if (response.Response != Responses.Success)
            return new(response.Response);

        // Obtener el perfil según el id de la identidad.
        var profile = await profilesData.ReadByIdentity(response.Model.Id);

        var httpResponse = new ReadOneResponse<AuthModel<ProfileModel>>()
        {
            Response = Responses.Success,
            Message = "Success"
        };

        switch (profile.Response)
        {
            case Responses.Success:
                break;

            case Responses.NotExistProfile:
                {
                    var res = await profilesData.Create(new()
                    {
                        Account = response.Model,
                        Profile = new()
                        {
                            IdentityId = response.Model.IdentityId,
                            Alias = response.Model.Name
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



        if (profile.Response == Responses.Success)
        {
            // Genera el token
            var tokenAcceso = Jwt.Generate(profile.Model);

            httpResponse.Token = tokenAcceso;
            httpResponse.Model.Profile = profile.Model;
        }

        httpResponse.Model.Account = response.Model;
        httpResponse.Model.TokenCollection = new()
        {
            { "identity",response.Token}
        };

        return httpResponse;
    }


    /// <summary>
    /// Buscar perfiles.
    /// </summary>
    /// <param name="pattern">Patron de búsqueda.</param>
    /// <param name="token">Token de acceso.</param>
    [HttpGet("search")]
    [RateLimit(requestLimit: 6, timeWindowSeconds: 60, blockDurationSeconds: 120)]
    public async Task<HttpReadAllResponse<SessionModel<ProfileModel>>> Search([FromQuery] string pattern, [FromHeader] string token)
    {

        // Buscar las cuentas según un patron de búsqueda en LIN Identity.
        var accounts = await LIN.Access.Auth.Controllers.Account.Search(pattern, token);

        // Si no tiene acceso
        if (accounts.Response != Responses.Success)
            return new ReadAllResponse<SessionModel<ProfileModel>>
            {
                Response = Responses.Unauthorized,
                Message = "No tienes acceso a LIN Identity"
            };

        // Obtener id de las cuentas.
        var mappedIds = accounts.Models.Select(T => T.IdentityId).ToList();

        // Obtener perfiles.
        var profiles = await profilesData.ReadByIdentities(mappedIds);

        // Armar el resultado.
        var final = from P in profiles.Models
                    join A in accounts.Models
                    on P.IdentityId equals A.IdentityId
                    select new SessionModel<ProfileModel>
                    {
                        Account = A,
                        Profile = P
                    };

        // Retorna el resultado
        return new ReadAllResponse<SessionModel<ProfileModel>>
        {
            Response = Responses.Success,
            Models = [.. final]
        };

    }


    [LocalToken]
    [HttpGet("devices")]
    [RateLimit(requestLimit: 30, timeWindowSeconds: 30, blockDurationSeconds: 30)]
    public HttpReadAllResponse<DeviceOnAccountModel> Devices([FromHeader] string token)
    {

        JwtModel tokenInfo = HttpContext.Items[token] as JwtModel ?? new();

        var session = Mems.Sessions[tokenInfo.ProfileId];

        return new ReadAllResponse<DeviceOnAccountModel>(Responses.Success, session?.Devices);
    }

}