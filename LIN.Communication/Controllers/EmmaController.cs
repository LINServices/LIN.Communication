using System.Text;
using Roles = LIN.Communication.Services.Roles;

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
    public async Task<HttpReadOneResponse<ResponseIAModel>> Assistant([FromHeader] string tokenAuth, [FromBody] string consult)
    {



        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Add("token", tokenAuth);
        client.DefaultRequestHeaders.Add("useDefaultContext", true.ToString().ToLower());


        var request = new LIN.Types.Models.EmmaRequest
        {
            AppsMethods = """
            {
              "name": "#mensaje",
              "description": "Enviar mensaje a un usuario o grupo",
              "example":"#mensaje(1, '¿Hola Como estas?')",
              "parameters": {
                "properties": {
                  "id": {
                    "type": "number",
                    "description": "Id de la conversación"
                  },
                  "content": {
                    "type": "string",
                    "description": "Contenido del mensaje"
                  }
                },
                "required": [
                  "id",
                  "description"
                ]
              }
            }
            {
              "name": "#select",
              "description": "Abrir una conversación, cuando el usuario se refiera a abrir una conversación",
              "example":"#select(0)",
              "parameters": {
                "properties": {
                  "content": {
                    "type": "number",
                    "description": "Id de la conversación"
                  }
                }
              }
            }
            """,
            Asks = consult
        };



        StringContent stringContent = new(Newtonsoft.Json.JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        var result = await client.PostAsync("http://api.emma.linapps.co/emma", stringContent);


        var ss = await result.Content.ReadAsStringAsync();


       dynamic fin = Newtonsoft.Json.JsonConvert.DeserializeObject(ss);


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
    /// Emma IA.
    /// </summary>
    /// <param name="token">Token de acceso.</param>
    /// <param name="consult">Prompt.</param>
    [HttpGet]
    public async Task<HttpReadOneResponse<object>> RequestFromEmma([FromHeader] string tokenAuth)
    {

        // Validar token.
        var response = await LIN.Access.Auth.Controllers.Authentication.Login(tokenAuth);


        if (response.Response != Responses.Success)
        {
            return new ReadOneResponse<object>()
            {
                Model = "Este usuario no autenticado en LIN Allo."
            };
        }

        // 
        var profile = await Data.Profiles.ReadByAccount(response.Model.Id);


        if (profile.Response != Responses.Success)
        {
            return new ReadOneResponse<object>()
            {
                Model = "Este usuario no tiene una cuenta en LIN Allo."
            };
        }


        var getProf = Mems.Sessions[profile.Model.ID];

        if (getProf == null)
        {

            var convs = (await Data.Conversations.ReadAll(profile.Model.ID))?.Models.Select(t =>
            (t.Conversation.ID, t.Conversation.Name));

            getProf = new MemorySession()
            {
                Profile = profile.Model,
                Conversations = convs?.ToList() ?? [],
            };
            Mems.Sessions.Add(getProf);
        }


        var final = getProf?.StringOfConversations() ?? "No hay conversaciones";

        return new ReadOneResponse<object>()
        {
            Model = final,
            Response = Responses.Success
        };

    }




}