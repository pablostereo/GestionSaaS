namespace GestionSaaS.Negocio.Dtos;

public record EstadisticasDto(
    DateOnly Desde,
    DateOnly Hasta,
    string ZonaHoraria,
    BalanceDto Balance,
    List<VentasPorDiaDto> PorDiaSemana,
    List<VentasPorFranjaDto> PorFranjaHoraria,
    List<ProductoRankingDto> TopVendidos,
    List<ProductoRankingDto> TopRentables,
    List<ProductoSinMovimientoDto> SinMovimiento);

/// <summary>
/// Resultado = Ingresos − Costo de mercadería vendida − Gastos operativos.
/// El costo es el congelado en cada línea de venta (costo del producto al momento de vender).
/// </summary>
public record BalanceDto(
    int CantidadVentas,
    decimal Ingresos,
    decimal CostoMercaderiaVendida,
    decimal GananciaBruta,
    decimal GastosOperativos,
    decimal ResultadoNeto,
    decimal TicketPromedio);

/// <summary>Dia: 1 = lunes … 7 = domingo.</summary>
public record VentasPorDiaDto(int Dia, string Nombre, int Cantidad, decimal Monto);

/// <summary>Franja de 2 horas: HoraDesde incluida, HoraHasta excluida.</summary>
public record VentasPorFranjaDto(int HoraDesde, int HoraHasta, string Etiqueta, int Cantidad, decimal Monto);

public record ProductoRankingDto(
    int ProductoId,
    string CodigoSku,
    string Nombre,
    decimal Unidades,
    decimal Ingresos,
    decimal Ganancia,
    decimal? MargenPorcentaje);

public record ProductoSinMovimientoDto(
    int ProductoId,
    string CodigoSku,
    string Nombre,
    decimal StockActual,
    decimal ValorInmovilizado,
    DateTime? UltimaVenta);
