using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

public class VentaServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    /// <summary>
    /// Registra una venta: valida stock, congela precio y costo de cada producto, calcula totales
    /// y descuenta stock en una sola transacción.
    /// </summary>
    public async Task<VentaDto> RegistrarAsync(VentaCrearDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        if (dto.Items is null || dto.Items.Count == 0)
            throw new NegocioException("La venta debe tener al menos un producto.");
        if (dto.Items.Any(i => i.Cantidad <= 0))
            throw new NegocioException("Las cantidades deben ser mayores a cero.");

        // Agrupa por si el mismo producto viene en dos líneas
        var items = dto.Items
            .GroupBy(i => i.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        var ids = items.Select(i => i.ProductoId).ToList();
        var productos = await Db.Productos.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        var faltantes = ids.Where(id => !productos.ContainsKey(id)).ToList();
        if (faltantes.Count > 0)
            throw new NoEncontradoException($"Productos inexistentes: {string.Join(", ", faltantes)}.");

        var sinStock = items
            .Where(i => productos[i.ProductoId].StockActual < i.Cantidad)
            .Select(i => $"{productos[i.ProductoId].Nombre} (disponible {productos[i.ProductoId].StockActual})")
            .ToList();
        if (sinStock.Count > 0)
            throw new NegocioException($"Stock insuficiente: {string.Join("; ", sinStock)}.");

        var venta = new Venta { MedioPago = dto.MedioPago, Fecha = DateTime.UtcNow };

        foreach (var item in items)
        {
            var producto = productos[item.ProductoId];
            var subtotal = Math.Round(item.Cantidad * producto.PrecioVenta, 2);

            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                CostoUnitario = producto.CostoActual,   // costo histórico: queda fijo aunque el producto cambie
                Subtotal = subtotal
            });

            producto.StockActual -= item.Cantidad;
        }

        venta.TotalVenta = venta.Detalles.Sum(d => d.Subtotal);
        Db.Ventas.Add(venta);

        await using var tx = await Db.Database.BeginTransactionAsync(ct);
        try
        {
            await Db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NegocioException("El stock fue modificado por otra venta al mismo tiempo. Reintente.");
        }

        return await ObtenerAsync(venta.Id, ct);
    }

    public async Task<VentaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var venta = await Consulta().FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NoEncontradoException($"Venta {id} no encontrada.");

        return ADto(venta);
    }

    public async Task<List<VentaDto>> ListarAsync(DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var query = Consulta();
        if (desde.HasValue) query = query.Where(v => v.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(v => v.Fecha < hasta.Value.Date.AddDays(1));

        var ventas = await query.OrderByDescending(v => v.Fecha).Take(500).ToListAsync(ct);
        return ventas.Select(ADto).ToList();
    }

    // ---------- privados ----------

    private IQueryable<Venta> Consulta() =>
        Db.Ventas.AsNoTracking().Include(v => v.Detalles).ThenInclude(d => d.Producto);

    private static VentaDto ADto(Venta v)
    {
        var items = v.Detalles
            .Select(d =>
            {
                var costo = Math.Round(d.CostoUnitario * d.Cantidad, 2);
                return new VentaItemDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, d.CostoUnitario,
                                        d.Subtotal, d.Subtotal - costo);
            })
            .ToList();

        var costoTotal = items.Sum(i => i.Subtotal - i.Ganancia);
        return new(v.Id, v.Fecha, v.TotalVenta, v.MedioPago, costoTotal, v.TotalVenta - costoTotal, items);
    }
}
