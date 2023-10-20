using LIN.Access.OpenIA.ModelsData;
using LIN.Communication.Memory;

namespace LIN.Communication.Controllers;


[Route("emma")]
public class EmmaController : ControllerBase
{





    [HttpPost]
    public async Task<HttpReadOneResponse<Message>> ReadAll([FromHeader] string token, [FromBody] string consult)
    {

        var (isValid, profileID, _, alias) = Jwt.Validate(token);

        if (!isValid)
        {
            return new ReadOneResponse<Message>()
            {
                Response = Responses.Unauthorized
            };
        }

        var getProf = Mems.Sessions[profileID] ;


        var emma = new Access.OpenIA.IA(Configuration.GetConfiguration("openIa:key"));

        // Carga el modelo
        emma.LoadWho();
        emma.LoadRecomendations();
        emma.LoadCommands();
        emma.LoadPersonality();
        emma.LoadSomething($""" 
                           Estas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
                           Estos son los nombres de los chats que tiene el usuario: {getProf.StringOfConversations()}
                           Recuerda que si el usuario quiere mandar un mensaje a un usuario/grupo/team etc, primero busca en su lista de nombres de chats
                           """);

        emma.LoadSomething($""" 
                           El alias del usuario es '{alias}'
                           """);

        var result = await emma.Respond(consult);

        return new ReadOneResponse<Message>()
        {
            Model = result,
            Response = Responses.Success
        };

    }




}