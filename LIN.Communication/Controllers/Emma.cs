using LIN.Access.OpenIA.ModelsData;
using LIN.Types.Communication.Models;

namespace LIN.Communication.Controllers;


[Route("emma")]
public class EmmaController : ControllerBase
{



   

    [HttpPost]
    public async Task<HttpReadOneResponse<Message>> ReadAll([FromBody] string consult)
    {

        var emma = new Access.OpenIA.IA(Configuration.GetConfiguration("openIa:key"));

        // Carga el modelo
        emma.LoadWho();
        emma.LoadRecomendations();
        emma.LoadCommands();
        emma.LoadPersonality();
        emma.LoadSomething(""" 
                           Estas en el contexto de LIN Allo, la app de comunicación de LIN Platform.
                           Estos son los nombres de los chats que tiene el usuario
                           -'Admins'
                           -'los cancheros'
                           -'Familia'
                           -'Juan Jose'
                           -'Elena'
                           Recuerda que si el usuario quiere mandar un mensaje a un usuario/grupo/team etc, primero busca en su lista de nombres de chats
                           """);

        var result = await emma.Respond(consult);

        return new ReadOneResponse<Message>()
        {
            Model = result,
            Response = Responses.Success
        };

    }




}