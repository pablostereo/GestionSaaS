using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

/// <summary>
/// Estadísticas del comercio a partir de Ventas, DetallesVenta, Productos y Gastos.
/// Todas las consultas pasan por los filtros globales de AppDbContext: solo se ven datos del cliente actual.
/// Los totales se calculan en memoria (SQLite no suma decimales en la base).
/// </summary>
public class EstadisticaServicio(AppDbContext db, IClienteActual clienteActual)
    : ServicioBase(db, clienteActual)
{
    private const int DiasPorDefecto = 30;
    private const int DiasMaximos = 366;
    private const int TamanoRanking = 5;
    private const int HorasPorFranja = 2;

    private static readonly string[] NombresDias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

    /// <param name="desde">Primer día (en hora local del comercio). Por defecto: hace 29 días.</param>
    /// <param name="hasta">Último día, incluido. Por defecto: hoy.</param>
    /// <param name="utcOffsetMinutos">
    /// Diferencia con UTC informada por el navegador (Argentina: -180). Se usa en lugar de las zonas
    /// del sistema operativo porque Windows puede tener reglas de horario de verano desactualizadas.
    /// </param>
    public async Task<EstadisticasDto> ObtenerAsync(DateOnly? desde, DateOnly? hasta, int? utcOffsetMinutos, CancellationToken ct = default)
    {
        await ValidarClienteAsync(ct);

        var zona = CrearZona(utcOffsetMinutos ?? Constantes.UtcOffsetMinutosPorDefecto);
        var hoy = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona));

        var fin = hasta ?? hoy;
        var inicio = desde ?? fin.AddDays(-(DiasPorDefecto - 1));
        if (inicio > fin)
            throw new NegocioException("La fecha desde no puede ser posterior a la fecha hasta.");
        if (fin.DayNumber - inicio.DayNumber + 1 > DiasMaximos)
            throw new NegocioException($"El rango no puede superar {DiasMaximos} días.");

        // Límites del período en UTC (así están guardadas las fechas)
        var inicioUtc = AUtc(inicio, zona);
        var finUtc = AUtc(fin.AddDays(1), zona);   // excluido

        // ---------- Lecturas (todas filtradas por cliente) ----------
        var ventas = await Db.Ventas.AsNoTracking()
            .Where(v => v.Fecha >= inicioUtc && v.Fecha < finUtc)
            .Select(v => new { v.Fecha, v.TotalVenta })
            .ToListAsync(ct);

        var detalles = await Db.DetallesVenta.AsNoTracking()
            .Where(d => d.Venta.Fecha >= inicioUtc && d.Venta.Fecha < finUtc)
            .Select(d => new { d.ProductoId, d.Cantidad, d.PrecioUnitario, d.CostoUnitario, d.Subtotal })
            .ToListAsync(ct);

        var productos = await Db.Productos.AsNoTracking()
            .Select(p => new { p.Id, p.CodigoSku, p.Nombre, p.CostoActual, p.StockActual })
            .ToDictionaryAsync(p => p.Id, ct);

        var gastos = await Db.Gastos.AsNoTracking()
            .Where(g => g.Fecha >= inicioUtc && g.Fecha < finUtc)
            .Select(g => g.Monto)
            .ToListAsync(ct);

        // ---------- Balance ----------
        // CMV con el costo histórico congelado en cada línea de venta (no el costo actual del producto)
        var ingresos = ventas.Sum(v => v.TotalVenta);
        var cmv = Math.Round(detalles.Sum(d => d.CostoUnitario * d.Cantidad), 2);
        var totalGastos = gastos.Sum();

        var balance = new BalanceDto(
            CantidadVentas: ventas.Count,
            Ingresos: ingresos,
            CostoMercaderiaVendida: cmv,
            GananciaBruta: ingresos - cmv,
            GastosOperativos: totalGastos,
            ResultadoNeto: ingresos - cmv - totalGastos,
            TicketPromedio: ventas.Count == 0 ? 0 : Math.Round(ingresos / ventas.Count, 2));

        // ---------- Por día de la semana y franja horaria (en hora local) ----------
        var ventasLocales = ventas
            .Select(v => new { Fecha = TimeZoneInfo.ConvertTimeFromUtc(v.Fecha, zona), v.TotalVenta })
            .ToList();

        var porDia = Enumerable.Range(1, 7).Select(dia =>
        {
            var delDia = ventasLocales.Where(v => DiaLunesADomingo(v.Fecha.DayOfWeek) == dia).ToList();
            return new VentasPorDiaDto(dia, NombresDias[dia - 1], delDia.Count, delDia.Sum(v => v.TotalVenta));
        }).ToList();

        var porFranja = Enumerable.Range(0, 24 / HorasPorFranja).Select(i =>
        {
            int horaDesde = i * HorasPorFranja, horaHasta = horaDesde + HorasPorFranja;
            var enFranja = ventasLocales.Where(v => v.Fecha.Hour >= horaDesde && v.Fecha.Hour < horaHasta).ToList();
            return new VentasPorFranjaDto(horaDesde, horaHasta, $"{horaDesde:00}–{horaHasta:00}", enFranja.Count, enFranja.Sum(v => v.TotalVenta));
        }).ToList();

        // ---------- Rankings ----------
        var porProducto = detalles
            .GroupBy(d => d.ProductoId)
            .Where(g => productos.ContainsKey(g.Key))
            .Select(g =>
            {
                var p = productos[g.Key];
                var unidades = g.Sum(d => d.Cantidad);
                var ingresosProducto = g.Sum(d => d.Subtotal);
                // Ganancia real: (PrecioUnitario − CostoUnitario histórico) × Cantidad
                var ganancia = Math.Round(g.Sum(d => (d.PrecioUnitario - d.CostoUnitario) * d.Cantidad), 2);
                decimal? margen = ingresosProducto == 0 ? null : Math.Round(ganancia / ingresosProducto * 100, 1);
                return new ProductoRankingDto(p.Id, p.CodigoSku, p.Nombre, unidades, ingresosProducto, ganancia, margen);
            })
            .ToList();

        var topVendidos = porProducto
            .OrderByDescending(r => r.Unidades).ThenByDescending(r => r.Ingresos)
            .Take(TamanoRanking).ToList();

        var topRentables = porProducto
            .OrderByDescending(r => r.Ganancia).ThenByDescending(r => r.Unidades)
            .Take(TamanoRanking).ToList();

        // ---------- Sin movimiento: con stock y sin ventas en el período ----------
        var vendidos = detalles.Select(d => d.ProductoId).ToHashSet();
        var parados = productos.Values.Where(p => p.StockActual > 0 && !vendidos.Contains(p.Id)).ToList();

        var idsParados = parados.Select(p => p.Id).ToList();
        var ultimasVentas = await Db.DetallesVenta.AsNoTracking()
            .Where(d => idsParados.Contains(d.ProductoId))
            .Select(d => new { d.ProductoId, d.Venta.Fecha })
            .GroupBy(x => x.ProductoId)
            .Select(g => new { ProductoId = g.Key, Ultima = g.Max(x => x.Fecha) })
            .ToDictionaryAsync(x => x.ProductoId, x => x.Ultima, ct);

        var sinMovimiento = parados
            .Select(p => new ProductoSinMovimientoDto(
                p.Id, p.CodigoSku, p.Nombre, p.StockActual,
                Math.Round(p.StockActual * p.CostoActual, 2),
                ultimasVentas.TryGetValue(p.Id, out var ultima) ? DateTime.SpecifyKind(ultima, DateTimeKind.Utc) : null))
            .OrderByDescending(p => p.ValorInmovilizado)
            .ToList();

        return new EstadisticasDto(inicio, fin, zona.Id, balance, porDia, porFranja, topVendidos, topRentables, sinMovimiento);
    }

    // ---------- privados ----------

    private static int DiaLunesADomingo(DayOfWeek dia) => dia == DayOfWeek.Sunday ? 7 : (int)dia;

    private static DateTime AUtc(DateOnly dia, TimeZoneInfo zona) =>
        TimeZoneInfo.ConvertTimeToUtc(dia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zona);

    private static TimeZoneInfo CrearZona(int offsetMinutos)
    {
        if (offsetMinutos is < -12 * 60 or > 14 * 60)
            throw new NegocioException("Diferencia horaria inválida.");

        var offset = TimeSpan.FromMinutes(offsetMinutos);
        var nombre = $"UTC{(offset < TimeSpan.Zero ? "-" : "+")}{offset.Duration():hh\\:mm}";
        return TimeZoneInfo.CreateCustomTimeZone(nombre, offset, nombre, nombre);
    }
}
