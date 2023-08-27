namespace LIN.Communication.Data;


public class Context : DbContext
{


    /// <summary>
    /// Perfiles
    /// </summary>
    public DbSet<ProfileModel> Profiles { get; set; }


    /// <summary>
    /// Conversaciones
    /// </summary>
    public DbSet<ConversationModel> Conversaciones { get; set; }



   
    public DbSet<MemberChatModel> Members { get; set; }



    public DbSet<MessageModel> Mensajes { get; set; }






    /// <summary>
    /// Nuevo contexto a la base de datos
    /// </summary>
    public Context(DbContextOptions<Context> options) : base(options) { }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

      

    }



}
