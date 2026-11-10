using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Negocio.Dtos;

public record CompraItemCrearDto(int ProductoId, decimal Cantidad, decimal CostoUnitario);

/// <summary>
/// Mercaderia: enviar Items (el MontoTotal se calcula solo).
/// Gasto/Servicio/Otro: enviar MontoTotal, sin Items.
/// </summary>
public record CompraCrearDto(
    TipoCompra Tipo,
    decimal? MontoTotal,
    string? Descripcion,
    DateTime? Fecha,
    List<CompraItemCrearDto>? Items);

public record CompraItemDto(int ProductoId, string Producto, decimal Cantidad, decimal CostoUnitario, decimal Subtotal);

public record CompraDto(int Id, DateTime Fecha, TipoCompra Tipo, decimal MontoTotal, string? Descripcion, List<CompraItemDto> Items);
