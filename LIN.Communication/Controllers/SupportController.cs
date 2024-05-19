using LIN.Communication.Services.Interfaces;

namespace LIN.Communication.Controllers;

[Route("support")]
public class SupportController(IIAService service) : ControllerBase
{


    [HttpGet]
    [Obsolete("Just Testing")]
    public async Task<IActionResult> UpdateIAData()
    {

        if (service is IAService realService)
        {
            realService.Clean();
        }

        return await Task.FromResult(Ok());

    }

}