namespace LIN.Communication.Services.Iam;

public interface IIamService
{
    public Task<IamLevels> Validate(int profile, int conversation);
}