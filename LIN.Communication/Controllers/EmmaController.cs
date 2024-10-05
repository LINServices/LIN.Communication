using LIN.Communication.Services.Interfaces;
using System.Text;

namespace LIN.Communication.Controllers;

[Route("[controller]")]
public class EmmaController(IIAService ia, Persistence.Data.Conversations conversationData, Persistence.Data.Profiles profilesData, IConfiguration configuration) : ControllerBase
{

    /// <summary>
    /// Consulta a la asistente virtual.
    /// </summary>
    /// <param name="tokenAuth">Token de identidad.</param>
    /// <param name="consult">Query.</param>
    [HttpPost]
    public async Task<HttpReadOneResponse<ResponseIAModel>> Assistant([FromHeader] string tokenAuth, [FromBody] string consult)
    {

        // Cliente HTTP.
        HttpClient client = new();

        // Headers.
        client.DefaultRequestHeaders.Add("token", tokenAuth);
        client.DefaultRequestHeaders.Add("useDefaultContext", true.ToString().ToLower());

        // Request.
        var request = new LIN.Types.Models.EmmaRequest
        {
            App = configuration["app:name"],
            Prompt = consult
        };

        // Contenido.
        StringContent stringContent = new(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Petici�n.
        var result = await client.PostAsync(configuration["services:emma"], stringContent);

        // Obtener contenido.
        var content = await result.Content.ReadAsStringAsync();

        // Obtener objeto.
        dynamic? fin = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

        // Respuesta
        return new ReadOneResponse<ResponseIAModel>()
        {
            Model = new()
            {
                IsSuccess = true,
                Content = fin?.result
            },
            Response = Responses.Success
        };

    }


    /// <summary>
    /// Emma IA desde el servicio Emma.
    /// </summary>
    /// <param name="tokenAuth">Token de identidad.</param>
    /// <param name="includeMethods">Incluir m�todos.</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<object>> RequestFromEmma([FromHeader] string tokenAuth, [FromHeader] bool includeMethods)
    {

        // Validar token.
        var response = await LIN.Access.Auth.Controllers.Authentication.Login(tokenAuth);

        // Validar en Auth.
        if (response.Response != Responses.Success)
        {
            return new ReadOneResponse<object>()
            {
                Model = "Este usuario no autenticado en LIN Allo."
            };
        }

        // Obtener el perfil.
        var profile = await profilesData.ReadByIdentity(response.Model.Id);

        if (profile.Response != Responses.Success)
        {
            return new ReadOneResponse<object>()
            {
                Model = "Este usuario no tiene una cuenta en LIN Allo."
            };
        }


        var getProf = Mems.Sessions[profile.Model.ID];

        if (getProf is null)
        {

            var convs = (await conversationData.ReadAll(profile.Model.ID))?.Models.Select(t =>
            (t.Conversation.ID, t.Conversation.Name));

            getProf = new MemorySession()
            {
                Profile = profile.Model,
                Conversations = convs?.ToList() ?? [],
            };
            Mems.Sessions.Add(getProf);
        }


        string final = ia.GetWith(getProf?.StringOfConversations() ?? string.Empty);

        final += includeMethods
                 ? ia.GetActions()
                 : ia.GetDefault();

        return new ReadOneResponse<object>()
        {
            Model = final,
            Response = Responses.Success
        };

    }

}