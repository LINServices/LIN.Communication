namespace LIN.Communication.Data;


public class Context : DbContext
{

    /// <summary>
    /// Tabla de perfiles.
    /// </summary>
    public DbSet<ProfileModel> Profiles { get; set; }


    /// <summary>
    /// Tabla de conversaciones.
    /// </summary>
    public DbSet<ConversationModel> Conversaciones { get; set; }


    /// <summary>
    /// Tabla de miembros.
    /// </summary>
    public DbSet<MemberChatModel> Members { get; set; }


    /// <summary>
    /// Tabla de mensajes
    /// </summary>
    public DbSet<MessageModel> Mensajes { get; set; }



    /// <summary>
    /// Nuevo contexto a la base de datos
    /// </summary>
    public Context(DbContextOptions<Context> options) : base(options) { }


}
