using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Entidades.Datos;

/// <summary>Datos de prueba para arrancar el sistema con una base vacía.</summary>
public static class DatosDemo
{
    /// <param name="hashear">Función que hashea contraseñas (la provee la capa de Negocio).</param>
    public static async Task CargarAsync(AppDbContext db, Func<string, string> hashear)
    {
        // --- Administrador ---
        if (!await db.Usuarios.AnyAsync(u => u.Rol == RolUsuario.Admin))
        {
            db.Usuarios.Add(new Usuario
            {
                Email = Constantes.AdminDemoEmail,
                Nombre = "Administrador",
                ContrasenaHash = hashear(Constantes.AdminDemoContrasena),
                Rol = RolUsuario.Admin
            });
            await db.SaveChangesAsync();
        }

        // --- Cliente demo con productos ---
        if (await db.Clientes.AnyAsync(c => c.Id == Constantes.ClienteDemoId))
            return;

        var id = Constantes.ClienteDemoId;

        db.Clientes.Add(new Cliente
        {
            Id = id,
            NombreComercio = "Comercio Demo",
            Email = Constantes.ClienteDemoEmail
        });

        db.Usuarios.Add(new Usuario
        {
            Email = Constantes.ClienteDemoEmail,
            Nombre = "Comercio Demo",
            ContrasenaHash = hashear(Constantes.ClienteDemoContrasena),
            Rol = RolUsuario.Cliente,
            ClienteId = id
        });

        db.Productos.AddRange(
            new Producto { ClienteId = id, CodigoSku = "ALM-001", Nombre = "Yerba 1kg",       PrecioVenta = 4500, CostoActual = 3200, StockActual = 40, StockMinimo = 10 },
            new Producto { ClienteId = id, CodigoSku = "ALM-002", Nombre = "Azúcar 1kg",      PrecioVenta = 1500, CostoActual = 1000, StockActual = 8,  StockMinimo = 10 },
            new Producto { ClienteId = id, CodigoSku = "BEB-001", Nombre = "Gaseosa 2.25L",   PrecioVenta = 2800, CostoActual = 1900, StockActual = 24, StockMinimo = 6 },
            new Producto { ClienteId = id, CodigoSku = "LIM-001", Nombre = "Lavandina 1L",    PrecioVenta = 1200, CostoActual = 750,  StockActual = 15, StockMinimo = 5 });

        await db.SaveChangesAsync();
    }
}
