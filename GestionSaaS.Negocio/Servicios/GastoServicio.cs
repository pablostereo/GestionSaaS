using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Entidades.Enums;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

/// <summary>
/// Gastos / egresos operativos (luz, alquiler, impuestos…).
/// Solo registran la salida de dinero: no tocan productos ni stock.
/// </summary>
public class GastoServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    public async Task<List<GastoDto>> ListarAsync(DateTime? desde, DateTime? hasta, CategoriaGasto? categoria, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var query = Db.Gastos.AsNoTracking();
        if (desde.HasValue) query = query.Where(g => g.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(g => g.Fecha < hasta.Value.Date.AddDays(1));
        if (categoria.HasValue) query = query.Where(g => g.Categoria == categoria.Value);

        var gastos = await query.OrderByDescending(g => g.Fecha).ThenByDescending(g => g.Id).Take(500).ToListAsync(ct);
        return gastos.Select(ADto).ToList();
    }

    public async Task<GastoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        return ADto(await BuscarAsync(id, ct));
    }

    public async Task<GastoDto> RegistrarAsync(GastoGuardarDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        Validar(dto);

        var gasto = new Gasto();
        Copiar(dto, gasto);

        Db.Gastos.Add(gasto);
        await Db.SaveChangesAsync(ct);
        return ADto(gasto);
    }

    public async Task<GastoDto> ActualizarAsync(int id, GastoGuardarDto dto, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        Validar(dto);

        var gasto = await BuscarAsync(id, ct);
        Copiar(dto, gasto);

        await Db.SaveChangesAsync(ct);
        return ADto(gasto);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);
        Db.Gastos.Remove(await BuscarAsync(id, ct));
        await Db.SaveChangesAsync(ct);
    }

    // ---------- privados ----------

    private async Task<Gasto> BuscarAsync(int id, CancellationToken ct) =>
        await Db.Gastos.FirstOrDefaultAsync(g => g.Id == id, ct)
        ?? throw new NoEncontradoException($"Gasto {id} no encontrado.");

    private static void Validar(GastoGuardarDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descripcion)) throw new NegocioException("La descripción es obligatoria.");
        if (dto.Descripcion.Trim().Length > 300) throw new NegocioException("La descripción no puede superar 300 caracteres.");
        if (dto.Monto <= 0) throw new NegocioException("El monto debe ser mayor a cero.");
        if (dto.Fecha == default) throw new NegocioException("La fecha es obligatoria.");
        if (dto.Fecha > DateTime.UtcNow.AddDays(1)) throw new NegocioException("La fecha del gasto no puede ser futura.");
        if (!Enum.IsDefined(dto.Categoria)) throw new NegocioException("Categoría inválida.");
        if (!Enum.IsDefined(dto.MedioPago)) throw new NegocioException("Medio de pago inválido.");
    }

    private static void Copiar(GastoGuardarDto dto, Gasto gasto)
    {
        gasto.Descripcion = dto.Descripcion.Trim();
        gasto.Monto = dto.Monto;
        gasto.Fecha = dto.Fecha;
        gasto.Categoria = dto.Categoria;
        gasto.MedioPago = dto.MedioPago;
    }

    private static GastoDto ADto(Gasto g) => new(g.Id, g.Fecha, g.Descripcion, g.Monto, g.Categoria, g.MedioPago);
}
