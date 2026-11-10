using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Entidades.Enums;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

public class CompraServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    /// <summary>
    /// Registra una compra. Si es de Mercaderia: suma stock, actualiza el costo
    /// de cada producto y calcula el total, todo en una sola transacción.
    /// </summary>
    public async Task<CompraDto> RegistrarAsync(CompraCrearDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        if (dto.Descripcion?.Length > 500)
            throw new NegocioException("La descripción no puede superar 500 caracteres.");

        var compra = new Compra
        {
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion?.Trim(),
            Fecha = dto.Fecha ?? DateTime.UtcNow
        };

        var items = dto.Items ?? [];

        if (dto.Tipo == TipoCompra.Mercaderia)
            await CargarMercaderiaAsync(compra, items, ct);
        else
        {
            if (items.Count > 0)
                throw new NegocioException("Solo las compras de Mercaderia pueden llevar productos.");
            if (dto.MontoTotal is not > 0)
                throw new NegocioException("El monto debe ser mayor a cero.");

            compra.MontoTotal = dto.MontoTotal.Value;
        }

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

    public async Task<List<CompraDto>> ListarAsync(DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var query = Consulta();
        if (desde.HasValue) query = query.Where(c => c.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(c => c.Fecha < hasta.Value.Date.AddDays(1));

        var compras = await query.OrderByDescending(c => c.Fecha).Take(500).ToListAsync(ct);
        return compras.Select(ADto).ToList();
    }

    // ---------- privados ----------

    private async Task CargarMercaderiaAsync(Compra compra, List<CompraItemCrearDto> items, CancellationToken ct)
    {
        if (items.Count == 0)
            throw new NegocioException("Una compra de Mercaderia debe tener al menos un producto.");
        if (items.Any(i => i.Cantidad <= 0))
            throw new NegocioException("Las cantidades deben ser mayores a cero.");
        if (items.Any(i => i.CostoUnitario < 0))
            throw new NegocioException("El costo unitario no puede ser negativo.");

        var ids = items.Select(i => i.ProductoId).Distinct().ToList();
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

    private IQueryable<Compra> Consulta() =>
        Db.Compras.AsNoTracking().Include(c => c.Detalles).ThenInclude(d => d.Producto);

    private static CompraDto ADto(Compra c) =>
        new(c.Id, c.Fecha, c.Tipo, c.MontoTotal, c.Descripcion,
            c.Detalles.Select(d => new CompraItemDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.CostoUnitario, d.Subtotal)).ToList());
}
