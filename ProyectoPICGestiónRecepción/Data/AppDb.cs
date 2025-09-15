using ProyectoPICGestiónRecepción.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

public class AppDbContext : DbContext
{
    public AppDbContext() : base("name=AppDb")
    {
        // Evitar que EF pluralice nombres de tablas
        this.Configuration.LazyLoadingEnabled = true;
    }

    public DbSet<Vehiculo> Vehiculos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Representante> Representantes { get; set; }
    public DbSet<ResponsableEstiba> Responsables { get; set; }
    public DbSet<RegistroEstiba> Registros { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        // Evita la pluralización automática
        modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        base.OnModelCreating(modelBuilder);
    }
}


