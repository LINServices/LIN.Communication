namespace LIN.Communication.Services;

[Obsolete("Se debe actualizar estos fragmentos.")]
public class IAService : Interfaces.IIAService
{

    private string Data { get; set; } = string.Empty;
    private string Default { get; set; } = string.Empty;
    private string Conversations { get; set; } = string.Empty;


    /// <summary>
    /// Obtener las instrucciones base.
    /// </summary>
    public string GetActions()
    {
        if (string.IsNullOrWhiteSpace(Data))
            Data = File.ReadAllText("wwwroot/Actions.ia");

        return Data ?? string.Empty;
    }


    /// <summary>
    /// Obtener las instrucciones base.
    /// </summary>
    public string GetDefault()
    {

        if (string.IsNullOrWhiteSpace(Data))
            Default = File.ReadAllText("wwwroot/Default.ia");

        return Default ?? string.Empty;

    }


    /// <summary>
    /// Obtener la data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public string GetWith(string data)
    {

        if (string.IsNullOrWhiteSpace(Data))
            Conversations = File.ReadAllText("wwwroot/Conversations.ia");

        return Conversations.Replace("[DATA]", data) ?? string.Empty;

    }


    /// <summary>
    /// Obtener la data.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public void Clean()
    {
        Conversations = null!;
        Default = null!;
        Data = null!;
    }

}