namespace LIN.Communication.Services.Iam;


public class Conversation
{


    /// <summary>
    /// Validar el acceso.
    /// </summary>
    /// <param name="profile">Id del perfil.</param>
    /// <param name="conversation">Id de la conversación.</param>
    public async static Task<IamLevels> Validate(int profile, int conversation)
    {
        try
        {

            // Contexto de conexión a la bd.
            var (context, contextKey) = Conexión.GetOneConnection();

            // Consulta.
            var have = await (from member in context.DataBase.Members
                              where member.Profile.ID == profile
                              && member.Conversation.ID == conversation
                              select member).FirstOrDefaultAsync();

            // Cerrar la conexión.
            context.CloseActions(contextKey);

            // No existe.
            if (have == null)
                return IamLevels.NotAccess;

            // Administrador.
            if (have.Rol == MemberRoles.Admin)
                return IamLevels.Privileged;

            // Visualizador.
            return IamLevels.Visualizer;
        }
        catch
        {
        }
        return IamLevels.NotAccess;
    }


}
