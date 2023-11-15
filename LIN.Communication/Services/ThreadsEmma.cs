namespace LIN.Communication.Services;


public class ThreadsEmma
{

    /// <summary>
    /// Lista de threads.
    /// </summary>
    public static Dictionary<string, List<ThreadMessage>> Threads { get; set; } = [];

}


public class ThreadMessage(string content, Roles rol)
{

    public string Content => content;
    public Roles Rol => rol;

}


public enum Roles
{
    User,
    Emma,
    System
}