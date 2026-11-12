using GestionSaaS.Entidades.Comun;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Entidades.Datos;

public class AppDbContext : DbContext
{
    private readonly IClienteActual _clienteActual;

    public AppDbContext(DbContextOptions<AppDbContext> options, IClienteActual clienteActual)
        : base(options)
    {
        _clienteActual = clienteActual;
    }

    // EF lo evalúa por instancia de contexto (por request)
    private Guid ClienteId => _clienteActual.ClienteId;

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Gasto> Gastos => Set<Gasto>();

    // Todas las fechas se guardan en UTC y se leen marcadas como UTC
    // (SQLite y SQL Server no guardan el "Kind" del DateTime)
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<FechaUtcConverter>();
    }

    private sealed class FechaUtcConverter() : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Cliente ----------
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasIndex(x => x.Email).IsUnique();
        });

        // ---------- Usuario ----------
        // Sin filtro por cliente: el login necesita buscar usuarios de todos los clientes
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasIndex(x => x.Email).IsUnique();

            e.Property(x => x.Rol)
             .HasConversion<string>()
             .HasMaxLength(20);

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Usuarios)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Producto ----------
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Productos");

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Productos)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);

            // SKU único por cliente (no global)
            e.HasIndex(x => new { x.ClienteId, x.CodigoSku }).IsUnique();
            e.HasIndex(x => new { x.ClienteId, x.Nombre });

            // SQL Server genera el RowVersion solo; en SQLite/Memoria lo genera SaveChangesAsync
            if (!Database.IsSqlServer())
                e.Property(x => x.RowVersion).ValueGeneratedNever();

            e.HasQueryFilter(x => x.ClienteId == ClienteId);
        });

        // ---------- Venta ----------
        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Ventas");

            e.Property(x => x.MedioPago)
             .HasConversion<string>()
             .HasMaxLength(30);

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Ventas)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.ClienteId, x.Fecha });

            e.HasQueryFilter(x => x.ClienteId == ClienteId);
        });

        // ---------- DetalleVenta ----------
        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.ToTable("DetallesVenta");

            e.HasOne(x => x.Venta)
             .WithMany(v => v.Detalles)
             .HasForeignKey(x => x.VentaId)
             .OnDelete(DeleteBehavior.Cascade);   // borrar venta => borra sus líneas

            e.HasOne(x => x.Producto)
             .WithMany(p => p.DetallesVenta)
             .HasForeignKey(x => x.ProductoId)
             .OnDelete(DeleteBehavior.Restrict);  // no se borra un producto con ventas

            e.HasIndex(x => x.ProductoId);

            // Hereda el cliente desde la cabecera de la venta
            e.HasQueryFilter(x => x.Venta.ClienteId == ClienteId);
        });

        // ---------- Proveedor ----------
        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedores");

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Proveedores)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);

            // Nombre único por cliente
            e.HasIndex(x => new { x.ClienteId, x.Nombre }).IsUnique();

            e.HasQueryFilter(x => x.ClienteId == ClienteId);
        });

        // ---------- Compra (reposición de mercadería) ----------
        modelBuilder.Entity<Compra>(e =>
        {
            e.ToTable("Compras");

            e.HasOne(x => x.Proveedor)
             .WithMany(p => p.Compras)
             .HasForeignKey(x => x.ProveedorId)
             .OnDelete(DeleteBehavior.Restrict);  // no se borra un proveedor con compras

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Compras)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.ClienteId, x.Fecha });

            e.HasQueryFilter(x => x.ClienteId == ClienteId);
        });

        // ---------- DetalleCompra ----------
        modelBuilder.Entity<DetalleCompra>(e =>
        {
            e.ToTable("DetallesCompra");

            e.HasOne(x => x.Compra)
             .WithMany(c => c.Detalles)
             .HasForeignKey(x => x.CompraId)
             .OnDelete(DeleteBehavior.Cascade);   // borrar compra => borra sus líneas

            e.HasOne(x => x.Producto)
             .WithMany(p => p.DetallesCompra)
             .HasForeignKey(x => x.ProductoId)
             .OnDelete(DeleteBehavior.Restrict);  // no se borra un producto con compras

            e.HasIndex(x => x.ProductoId);

            // Hereda el cliente desde la cabecera de la compra
            e.HasQueryFilter(x => x.Compra.ClienteId == ClienteId);
        });

        // ---------- Gasto (egreso operativo, no toca stock) ----------
        modelBuilder.Entity<Gasto>(e =>
        {
            e.ToTable("Gastos");

            e.Property(x => x.Categoria)
             .HasConversion<string>()
             .HasMaxLength(30);

            e.Property(x => x.MedioPago)
             .HasConversion<string>()
             .HasMaxLength(30);

            e.HasOne(x => x.Cliente)
             .WithMany(c => c.Gastos)
             .HasForeignKey(x => x.ClienteId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.ClienteId, x.Fecha });

            e.HasQueryFilter(x => x.ClienteId == ClienteId);
        });
    }

    // Completa el ClienteId en los registros nuevos y bloquea cambios de cliente
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<IEntidadCliente>())
        {
            if (entry.State == EntityState.Added && entry.Entity.ClienteId == Guid.Empty)
                entry.Entity.ClienteId = ClienteId;

            if (entry.State == EntityState.Modified)
                entry.Property(x => x.ClienteId).IsModified = false;
        }

        // Control de concurrencia del stock fuera de SQL Server: nueva versión en cada cambio
        if (!Database.IsSqlServer())
        {
            foreach (var entry in ChangeTracker.Entries<Producto>())
                if (entry.State is EntityState.Added or EntityState.Modified)
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
        }

        return base.SaveChangesAsync(ct);
    }
}
