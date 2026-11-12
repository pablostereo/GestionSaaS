using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

/// <summary>Proveedores de mercadería del cliente. No se borran: se desactivan.</summary>
public class ProveedorServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    public async Task<List<ProveedorDto>> ListarAsync(string? buscar, bool soloActivos, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var query = Db.Proveedores.AsNoTracking();
        if (soloActivos) query = query.Where(p => p.Activo);
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(p => p.Nombre.Contains(buscar) || (p.Cuit != null && p.Cuit.Contains(buscar)));

        var proveedores = await query.OrderBy(p => p.Nombre).ToListAsync(ct);
        var resumen = await ResumenComprasAsync(ct);

        return proveedores.Select(p => ADto(p, resumen)).ToList();
    }

    public async Task<ProveedorDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        return ADto(await BuscarAsync(id, ct), await ResumenComprasAsync(ct));
    }

    public async Task<ProveedorDto> CrearAsync(ProveedorGuardarDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        await ValidarAsync(dto, null, ct);

        var proveedor = new Proveedor();
        Copiar(dto, proveedor);

        Db.Proveedores.Add(proveedor);
        await Db.SaveChangesAsync(ct);
        return ADto(proveedor, []);
    }

    public async Task<ProveedorDto> ActualizarAsync(int id, ProveedorGuardarDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        await ValidarAsync(dto, id, ct);

        var proveedor = await BuscarAsync(id, ct);
        Copiar(dto, proveedor);

        await Db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    public async Task<ProveedorDto> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var proveedor = await BuscarAsync(id, ct);
        proveedor.Activo = activo;

        await Db.SaveChangesAsync(ct);
        return await ObtenerAsync(id, ct);
    }

    // ---------- privados ----------

    private sealed record Resumen(int Cantidad, decimal Total, DateTime Ultima);

    // Se suma en memoria: SQLite no suma decimales en la base
    private async Task<Dictionary<int, Resumen>> ResumenComprasAsync(CancellationToken ct)
    {
        var compras = await Db.Compras.AsNoTracking()
            .Select(c => new { c.ProveedorId, c.MontoTotal, c.Fecha })
            .ToListAsync(ct);

        return compras.GroupBy(c => c.ProveedorId)
            .ToDictionary(g => g.Key, g => new Resumen(g.Count(), g.Sum(c => c.MontoTotal), g.Max(c => c.Fecha)));
    }

    private async Task<Proveedor> BuscarAsync(int id, CancellationToken ct) =>
        await Db.Proveedores.FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new NoEncontradoException($"Proveedor {id} no encontrado.");

    private async Task ValidarAsync(ProveedorGuardarDto dto, int? idActual, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new NegocioException("El nombre es obligatorio.");
        if (dto.Nombre.Trim().Length > 150) throw new NegocioException("El nombre no puede superar 150 caracteres.");
        if (dto.Cuit?.Trim().Length > 20) throw new NegocioException("El CUIT no puede superar 20 caracteres.");
        if (dto.Telefono?.Trim().Length > 50) throw new NegocioException("El teléfono no puede superar 50 caracteres.");
        if (dto.Email?.Trim().Length > 256) throw new NegocioException("El email no puede superar 256 caracteres.");

        var nombre = dto.Nombre.Trim();
        if (await Db.Proveedores.AnyAsync(p => p.Nombre == nombre && (idActual == null || p.Id != idActual), ct))
            throw new NegocioException($"Ya existe un proveedor llamado {nombre}.");
    }

    private static void Copiar(ProveedorGuardarDto dto, Proveedor p)
    {
        p.Nombre = dto.Nombre.Trim();
        p.Cuit = Limpiar(dto.Cuit);
        p.Telefono = Limpiar(dto.Telefono);
        p.Email = Limpiar(dto.Email);
    }

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static ProveedorDto ADto(Proveedor p, Dictionary<int, Resumen> resumen)
    {
        var r = resumen.GetValueOrDefault(p.Id);
        return new(p.Id, p.Nombre, p.Cuit, p.Telefono, p.Email, p.Activo, r?.Cantidad ?? 0, r?.Total ?? 0, r?.Ultima);
    }
}
