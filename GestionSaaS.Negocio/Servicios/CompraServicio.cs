using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

/// <summary>
/// Compras de reposición de mercadería. Afectan inventario: suman stock y actualizan el costo.
/// Los pagos operativos (luz, alquiler…) van por <see cref="GastoServicio"/>.
/// </summary>
public class CompraServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    /// <summary>
    /// Registra la compra, suma el stock y deja como costo de cada producto
    /// el de esta compra. Todo en una sola transacción.
    /// </summary>
    public async Task<CompraDto> RegistrarAsync(CompraCrearDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        if (dto.NumeroComprobante?.Trim().Length > 50)
            throw new NegocioException("El número de comprobante no puede superar 50 caracteres.");
        if (dto.Observaciones?.Trim().Length > 500)
            throw new NegocioException("Las observaciones no pueden superar 500 caracteres.");
        if (dto.Fecha > DateTime.UtcNow.AddDays(1))
            throw new NegocioException("La fecha de la compra no puede ser futura.");

        var proveedor = await Db.Proveedores.FirstOrDefaultAsync(p => p.Id == dto.ProveedorId, ct)
            ?? throw new NegocioException("Seleccione un proveedor válido.");
        if (!proveedor.Activo)
            throw new NegocioException($"El proveedor {proveedor.Nombre} está inactivo.");

        var compra = new Compra
        {
            ProveedorId = proveedor.Id,
            NumeroComprobante = Limpiar(dto.NumeroComprobante),
            Observaciones = Limpiar(dto.Observaciones),
            Fecha = dto.Fecha ?? DateTime.UtcNow
        };

        await CargarItemsAsync(compra, dto.Items ?? [], ct);

        Db.Compras.Add(compra);

        await using var tx = await Db.Database.BeginTransactionAsync(ct);
        try
        {
            await Db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NegocioException("El stock fue modificado por otra operación al mismo tiempo. Reintente.");
        }

        return await ObtenerAsync(compra.Id, ct);
    }

    public async Task<CompraDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var compra = await Consulta().FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NoEncontradoException($"Compra {id} no encontrada.");

        return ADto(compra);
    }

    public async Task<List<CompraDto>> ListarAsync(DateTime? desde, DateTime? hasta, int? proveedorId, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var query = Consulta();
        if (desde.HasValue) query = query.Where(c => c.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(c => c.Fecha < hasta.Value.Date.AddDays(1));
        if (proveedorId.HasValue) query = query.Where(c => c.ProveedorId == proveedorId.Value);

        var compras = await query.OrderByDescending(c => c.Fecha).Take(500).ToListAsync(ct);
        return compras.Select(ADto).ToList();
    }

    // ---------- privados ----------

    private async Task CargarItemsAsync(Compra compra, List<CompraItemCrearDto> items, CancellationToken ct)
    {
        if (items.Count == 0)
            throw new NegocioException("La compra debe tener al menos un producto.");
        if (items.Any(i => i.Cantidad <= 0))
            throw new NegocioException("Las cantidades deben ser mayores a cero.");
        if (items.Any(i => i.CostoUnitario < 0))
            throw new NegocioException("El costo unitario no puede ser negativo.");
        if (items.GroupBy(i => i.ProductoId).Any(g => g.Count() > 1))
            throw new NegocioException("Hay productos repetidos en la compra.");

        var ids = items.Select(i => i.ProductoId).ToList();
        var productos = await Db.Productos.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        var faltantes = ids.Where(id => !productos.ContainsKey(id)).ToList();
        if (faltantes.Count > 0)
            throw new NoEncontradoException($"Productos inexistentes: {string.Join(", ", faltantes)}.");

        foreach (var item in items)
        {
            var producto = productos[item.ProductoId];

            compra.Detalles.Add(new DetalleCompra
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                CostoUnitario = item.CostoUnitario,
                Subtotal = Math.Round(item.Cantidad * item.CostoUnitario, 2)
            });

            producto.StockActual += item.Cantidad;
            producto.CostoActual = item.CostoUnitario;   // último costo de compra
        }

        compra.MontoTotal = compra.Detalles.Sum(d => d.Subtotal);
    }

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private IQueryable<Compra> Consulta() =>
        Db.Compras.AsNoTracking()
            .Include(c => c.Proveedor)
            .Include(c => c.Detalles).ThenInclude(d => d.Producto);

    private static CompraDto ADto(Compra c) =>
        new(c.Id, c.Fecha, c.ProveedorId, c.Proveedor.Nombre, c.NumeroComprobante, c.Observaciones, c.MontoTotal,
            c.Detalles.Select(d => new CompraItemDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.CostoUnitario, d.Subtotal)).ToList());
}
