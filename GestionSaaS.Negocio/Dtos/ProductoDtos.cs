namespace GestionSaaS.Negocio.Dtos;

public record ProductoGuardarDto(
    string CodigoSku,
    string Nombre,
    decimal PrecioVenta,
    decimal CostoActual,
    decimal StockMinimo,
    decimal StockInicial = 0);   // solo se usa al crear

public record AjusteStockDto(decimal Cantidad, string? Motivo);   // + suma, - resta

public record ProductoDto(
    int Id,
    string CodigoSku,
    string Nombre,
    decimal PrecioVenta,
    decimal CostoActual,
    decimal StockActual,
    decimal StockMinimo,
    bool StockBajo);
