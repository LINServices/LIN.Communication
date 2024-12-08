namespace LIN.Communication.Persistence;

/// <summary>
/// Nuevo contexto a la base de datos
/// </summary>
public class Context(DbContextOptions<Context> options) : DbContext(options)
{

    /// <summary>
    /// Tabla de perfiles.
    /// </summary>
    public DbSet<ProfileModel> Profiles { get; set; }


    /// <summary>
    /// Tabla de conversaciones.
    /// </summary>
    public DbSet<ConversationModel> Conversations { get; set; }


    /// <summary>
    /// Tabla de miembros.
    /// </summary>
    public DbSet<MemberChatModel> Members { get; set; }


    /// <summary>
    /// Tabla de mensajes
    /// </summary>
    public DbSet<MessageModel> Messages { get; set; }


    /// <summary>
    /// Mensajes temporales.
    /// </summary>
    public DbSet<TempMessageModel> TempMessages { get; set; }


    /// <summary>
    /// Al crear.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tablas.
        modelBuilder.Entity<ProfileModel>().ToTable("profiles");
        modelBuilder.Entity<ConversationModel>().ToTable("conversations");
        modelBuilder.Entity<MemberChatModel>().ToTable("members");
        modelBuilder.Entity<MessageModel>().ToTable("messages");
        modelBuilder.Entity<TempMessageModel>().ToTable("temp_messages");

        base.OnModelCreating(modelBuilder);
    }

}