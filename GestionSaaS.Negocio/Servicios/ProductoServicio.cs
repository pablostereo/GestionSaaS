using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

public class ProductoServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    public async Task<List<ProductoDto>> ListarAsync(string? buscar, bool soloStockBajo, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var query = Db.Productos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(p => p.Nombre.Contains(buscar) || p.CodigoSku.Contains(buscar));

        if (soloStockBajo)
            query = query.Where(p => p.StockActual <= p.StockMinimo);

        return await query.OrderBy(p => p.Nombre).Select(p => ADto(p)).ToListAsync(ct);
    }

    public async Task<ProductoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        return ADto(await BuscarAsync(id, ct));
    }

    public async Task<ProductoDto> CrearAsync(ProductoGuardarDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        Validar(dto);
        if (dto.StockInicial < 0)
            throw new NegocioException("El stock inicial no puede ser negativo.");

        var producto = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            PrecioVenta = dto.PrecioVenta,
            CostoActual = dto.CostoActual,
            StockMinimo = dto.StockMinimo,
            StockActual = dto.StockInicial
        };
        Db.Productos.Add(producto);

        // SKU automático y correlativo por cliente. Si dos altas simultáneas toman el mismo
        // número, el índice único (ClienteId, CodigoSku) rechaza una y se reintenta con el siguiente.
        for (var intento = 1; ; intento++)
        {
            producto.CodigoSku = await SiguienteSkuAsync(ct);
            try
            {
                await Db.SaveChangesAsync(ct);
                return ADto(producto);
            }
            catch (DbUpdateException) when (intento < MaxIntentosSku)
            {
                Db.Entry(producto).State = EntityState.Detached;
                Db.Productos.Add(producto);
            }
        }
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ProductoGuardarDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        Validar(dto);

        var producto = await BuscarAsync(id, ct);   // el SKU no se modifica
        producto.Nombre = dto.Nombre.Trim();
        producto.PrecioVenta = dto.PrecioVenta;
        producto.CostoActual = dto.CostoActual;
        producto.StockMinimo = dto.StockMinimo;
        // El stock NO se edita acá: solo por ventas o ajustes

        await Db.SaveChangesAsync(ct);
        return ADto(producto);
    }

    /// <summary>Ajuste manual de stock (inventario, rotura, ingreso de mercadería).</summary>
    public async Task<ProductoDto> AjustarStockAsync(int id, AjusteStockDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        if (dto.Cantidad == 0)
            throw new NegocioException("La cantidad del ajuste no puede ser cero.");

        var producto = await BuscarAsync(id, ct);
        if (producto.StockActual + dto.Cantidad < 0)
            throw new NegocioException($"El ajuste deja el stock negativo (actual: {producto.StockActual}).");

        producto.StockActual += dto.Cantidad;

        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NegocioException("El stock fue modificado por otra operación. Reintente.");
        }

        return ADto(producto);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        var producto = await BuscarAsync(id, ct);

        if (await Db.DetallesVenta.AnyAsync(d => d.ProductoId == id, ct))
            throw new NegocioException("No se puede eliminar un producto que tiene ventas.");

        if (await Db.DetallesCompra.AnyAsync(d => d.ProductoId == id, ct))
            throw new NegocioException("No se puede eliminar un producto que tiene compras.");

        Db.Productos.Remove(producto);
        await Db.SaveChangesAsync(ct);
    }

    // ---------- privados ----------

    private const int LargoSku = 6;
    private const int MaxIntentosSku = 3;

    /// <summary>Próximo SKU del cliente actual: 000001, 000002… Ignora códigos con otro formato.</summary>
    private async Task<string> SiguienteSkuAsync(CancellationToken ct)
    {
        var existentes = await Db.Productos
            .Where(p => p.CodigoSku.Length == LargoSku)
            .Select(p => p.CodigoSku)
            .ToListAsync(ct);

        var ultimo = existentes
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return (ultimo + 1).ToString($"D{LargoSku}");
    }

    private async Task<Producto> BuscarAsync(int id, CancellationToken ct) =>
        await Db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new NoEncontradoException($"Producto {id} no encontrado.");

    private static void Validar(ProductoGuardarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))    throw new NegocioException("El nombre es obligatorio.");
        if (dto.PrecioVenta < 0)  throw new NegocioException("El precio de venta no puede ser negativo.");
        if (dto.CostoActual < 0)  throw new NegocioException("El costo no puede ser negativo.");
        if (dto.StockMinimo < 0)  throw new NegocioException("El stock mínimo no puede ser negativo.");
    }

    private static ProductoDto ADto(Producto p) =>
        new(p.Id, p.CodigoSku, p.Nombre, p.PrecioVenta, p.CostoActual, p.StockActual, p.StockMinimo,
            p.StockActual <= p.StockMinimo);
}
