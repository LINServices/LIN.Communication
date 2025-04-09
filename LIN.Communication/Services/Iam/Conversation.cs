using LIN.Communication.Persistence;

namespace LIN.Communication.Services.Iam;

public class Conversation(Context context) : IIamService
{

    /// <summary>
    /// Validar el acceso.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="conversation">Id de la conversación.</param>
    public async Task<IamLevels> Validate(int profile, int conversation)
    {
        try
        {
            // Buscar en los integrantes de una conversación si existe el perfil.
            var have = await (from member in context.Members
                              where member.Profile.Id == profile
                              && member.Conversation.Id == conversation
                              select member).FirstOrDefaultAsync();

            // Si no hay registros, no tiene acceso.
            if (have is null)
                return IamLevels.NotAccess;

            // Si es el administrador.
            if (have.Rol is MemberRoles.Admin)
                return IamLevels.Privileged;

            // Visualizador.
            return IamLevels.Visualizer;
        }
        catch (Exception)
        {
        }
        return IamLevels.NotAccess;
    }

}