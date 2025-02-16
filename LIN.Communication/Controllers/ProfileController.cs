namespace LIN.Communication.Controllers;

[Route("[controller]")]
public class ProfileController(Persistence.Data.Profiles profilesData) : ControllerBase
{

    /// <summary>
    /// Inicia una sesión con credenciales.
    /// </summary>
    /// <param name="user">Usuario único</param>
    /// <param name="password">Contraseña del usuario</param>
    /// <param name="app">Key de la app que solicita la información</param>
    [HttpGet("login")]
    [RateLimit(requestLimit: 5, timeWindowSeconds: 60, blockDurationSeconds: 120)]
    public async Task<HttpReadOneResponse<AuthModel<ProfileModel>>> Login([FromQuery] string user, [FromQuery] string password, [FromHeader] string? app)
    {

        // Validación de datos.
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            return new(Responses.InvalidParam);

        if (string.IsNullOrWhiteSpace(app))
            app = null;

        // Respuesta de autenticación
        var authResponse = await LIN.Access.Auth.Controllers.Authentication.Login(user, password, app);

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
        var profile = await profilesData.ReadByIdentity(authResponse.Model.IdentityId);

        switch (profile.Response)
        {
            case Responses.Success:
                break;

            case Responses.NotExistProfile:
                {
                    var res = await profilesData.Create(new()
                    {
                        Account = authResponse.Model,
                        Profile = new()
                        {
                            IdentityId = authResponse.Model.IdentityId,
                            Alias = authResponse.Model.Name
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


        var profile = await profilesData.ReadByIdentity(response.Model.Id);


        var httpResponse = new ReadOneResponse<AuthModel<ProfileModel>>()
        {
            Response = Responses.Success,
            Message = "Success",

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

        // Busca el acceso
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
            Models = final.ToList()
        };

    }

}