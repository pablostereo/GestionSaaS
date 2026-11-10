using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Entidades.Enums;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using GestionSaaS.Negocio.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

/// <summary>ABM de clientes (comercios). Solo para administradores: ve datos de todos los clientes.</summary>
public class ClienteServicio(AppDbContext db)
{
    public async Task<List<ClienteDto>> ListarAsync(CancellationToken ct = default)
    {
        var clientes = await db.Clientes.AsNoTracking().OrderBy(c => c.NombreComercio).ToListAsync(ct);
        return await ConEstadisticasAsync(clientes, ct);
    }

    public async Task<ClienteDto> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NoEncontradoException("Cliente no encontrado.");

        return (await ConEstadisticasAsync([cliente], ct))[0];
    }

    /// <summary>Crea el comercio y su usuario de acceso.</summary>
    public async Task<ClienteDto> CrearAsync(ClienteCrearDto dto, CancellationToken ct = default)
    {
        var (nombre, email) = await ValidarAsync(dto.NombreComercio, dto.Email, null, ct);
        Contrasenas.Validar(dto.Contrasena);

        var cliente = new Cliente { NombreComercio = nombre, Email = email };
        cliente.Usuarios.Add(new Usuario
        {
            Email = email,
            Nombre = nombre,
            ContrasenaHash = Contrasenas.Hashear(dto.Contrasena),
            Rol = RolUsuario.Cliente
        });

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(cliente.Id, ct);
    }

    /// <summary>Modifica los datos del comercio. El email de acceso se actualiza también.</summary>
    public async Task<ClienteDto> ActualizarAsync(Guid id, ClienteActualizarDto dto, CancellationToken ct = default)
    {
        var cliente = await BuscarAsync(id, ct);
        var (nombre, email) = await ValidarAsync(dto.NombreComercio, dto.Email, id, ct);

        var usuario = await UsuarioPrincipalAsync(cliente, ct);
        if (usuario is not null)
        {
            if (usuario.Email == cliente.Email) usuario.Email = email;
            if (usuario.Nombre == cliente.NombreComercio) usuario.Nombre = nombre;
        }

        cliente.NombreComercio = nombre;
        cliente.Email = email;

        await db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    /// <summary>Baja lógica / reactivación. Un cliente inactivo no puede ingresar ni operar.</summary>
    public async Task CambiarEstadoAsync(Guid id, bool activo, CancellationToken ct = default)
    {
        var cliente = await BuscarAsync(id, ct);
        cliente.Activo = activo;
        await db.SaveChangesAsync(ct);
    }

    public async Task RestablecerContrasenaAsync(Guid id, RestablecerContrasenaDto dto, CancellationToken ct = default)
    {
        Contrasenas.Validar(dto.Contrasena);

        var cliente = await BuscarAsync(id, ct);
        var usuario = await UsuarioPrincipalAsync(cliente, ct)
            ?? throw new NegocioException("El cliente no tiene usuario de acceso.");

        usuario.ContrasenaHash = Contrasenas.Hashear(dto.Contrasena);
        await db.SaveChangesAsync(ct);
    }

    // ---------- privados ----------

    private async Task<Cliente> BuscarAsync(Guid id, CancellationToken ct) =>
        await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct)
        ?? throw new NoEncontradoException("Cliente no encontrado.");

    private Task<Usuario?> UsuarioPrincipalAsync(Cliente cliente, CancellationToken ct) =>
        db.Usuarios.Where(u => u.ClienteId == cliente.Id).OrderBy(u => u.Id).FirstOrDefaultAsync(ct);

    private async Task<(string nombre, string email)> ValidarAsync(string? nombre, string? email, Guid? idActual, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new NegocioException("El nombre del comercio es obligatorio.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Trim().Length > 256)
            throw new NegocioException("El email no es válido.");

        var emailNormalizado = email.Trim().ToLowerInvariant();

        if (await db.Clientes.AnyAsync(c => c.Email == emailNormalizado && c.Id != idActual, ct))
            throw new NegocioException("Ya existe un cliente con ese email.");

        if (await db.Usuarios.AnyAsync(u => u.Email == emailNormalizado && (idActual == null || u.ClienteId != idActual), ct))
            throw new NegocioException("Ese email ya está en uso por otro usuario.");

        return (nombre.Trim(), emailNormalizado);
    }

    /// <summary>Agrega cantidad de productos, ventas, total vendido y último ingreso (de todos los clientes).</summary>
    private async Task<List<ClienteDto>> ConEstadisticasAsync(List<Cliente> clientes, CancellationToken ct)
    {
        var ids = clientes.Select(c => c.Id).ToList();

        var productos = await db.Productos.IgnoreQueryFilters()
            .Where(p => ids.Contains(p.ClienteId))
            .GroupBy(p => p.ClienteId)
            .Select(g => new { g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Cantidad, ct);

        // Se suma en memoria: SQLite no suma decimales de forma nativa
        var ventas = (await db.Ventas.IgnoreQueryFilters()
                .Where(v => ids.Contains(v.ClienteId))
                .Select(v => new { v.ClienteId, v.TotalVenta })
                .ToListAsync(ct))
            .GroupBy(v => v.ClienteId)
            .ToDictionary(g => g.Key, g => (Cantidad: g.Count(), Total: g.Sum(x => x.TotalVenta)));

        var ingresos = await db.Usuarios
            .Where(u => u.ClienteId != null && ids.Contains(u.ClienteId.Value))
            .GroupBy(u => u.ClienteId!.Value)
            .Select(g => new { g.Key, Ultimo = g.Max(u => u.UltimoIngreso) })
            .ToDictionaryAsync(x => x.Key, x => x.Ultimo, ct);

        return clientes.Select(c => new ClienteDto(
            c.Id, c.NombreComercio, c.Email, c.FechaAlta, c.Activo,
            ingresos.GetValueOrDefault(c.Id),
            productos.GetValueOrDefault(c.Id),
            ventas.TryGetValue(c.Id, out var v) ? v.Cantidad : 0,
            ventas.TryGetValue(c.Id, out var v2) ? v2.Total : 0)).ToList();
    }
}
