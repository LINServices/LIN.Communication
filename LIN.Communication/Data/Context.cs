namespace LIN.Inventory.Data;


public class Context : DbContext
{



    public DbSet<ProfileModel> Profiles { get; set; }



    public DbSet<ConversaciónModel> Conversaciones { get; set; }



    public DbSet<MessageModel> Mensajes { get; set; }






    /// <summary>
    /// Nuevo contexto a la base de datos
    /// </summary>
    public Context(DbContextOptions<Context> options) : base(options) { }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Indices y identidad
        modelBuilder.Entity<ProfileModel>()
           .HasIndex(e => e.Alias)
           .IsUnique();



    }



}
