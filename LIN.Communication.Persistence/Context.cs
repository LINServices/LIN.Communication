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


    public DbSet<DeviceModel> Devices { get; set; }
    public DbSet<MeetingMemberModel> MeetingMembers { get; set; }
    public DbSet<MeetingModel> Meetings { get; set; }


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


        modelBuilder.Entity<DeviceModel>(et =>
        {
            et.HasOne(e => e.MeetingMember)
            .WithMany(e=>e.Devices)
            .HasForeignKey(t => t.MeetingMemberId);

        });

        modelBuilder.Entity<MeetingMemberModel>(et =>
        {
            et.HasOne(e => e.ProfileModel)
            .WithMany()
            .HasForeignKey(t => t.ProfileId);

            et.HasOne(e => e.Meeting)
          .WithMany(e=>e.Members)
          .HasForeignKey(t => t.MeetingId);

        });


        modelBuilder.Entity<MeetingModel>(et =>
        {
            et.HasOne(e => e.Conversation)
            .WithMany()
            .HasForeignKey(t => t.ConversationId);

            et.HasMany(e => e.Members)
          .WithOne(e => e.Meeting)
          .HasForeignKey(t => t.MeetingId);

        });


        base.OnModelCreating(modelBuilder);
    }

}