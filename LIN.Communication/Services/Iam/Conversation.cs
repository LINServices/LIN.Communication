﻿using LIN.Communication.Persistence;

namespace LIN.Communication.Services.Iam;

public class Conversation (Context context) : IIamService
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
            // Consulta.
            var have = await (from member in context.Members
                              where member.Profile.ID == profile
                              && member.Conversation.ID == conversation
                              select member).FirstOrDefaultAsync();

            // No existe.
            if (have == null)
                return IamLevels.NotAccess;

            // Administrador.
            if (have.Rol == MemberRoles.Admin)
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