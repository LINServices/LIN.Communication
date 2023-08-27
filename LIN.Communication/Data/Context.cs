using LIN.Types.Auth.Models;

namespace LIN.Communication.Data;


public class Context : DbContext
{



    public DbSet<ProfileModel> Profiles { get; set; }



    public DbSet<ConversationModel> Conversaciones { get; set; }



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


        modelBuilder.Entity<ConversationModel>()
          .HasOne(p => p.UsuarioA)
          .WithMany()
          .HasForeignKey(p => p.UsuarioAID)
          .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationModel>()
         .HasOne(p => p.UsuarioB)
         .WithMany()
         .HasForeignKey(p => p.UsuarioBID)
         .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<ConversationModel>()
           .HasKey(a => a.ID);


        modelBuilder.Entity<ConversationModel>()
            .HasKey(a => new { a.ID, a.UsuarioAID, a.UsuarioBID });

    }



}
