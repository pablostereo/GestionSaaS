using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Negocio.Dtos;

// El precio y el costo NO se reciben: los toma el sistema del producto al registrar la venta
public record VentaItemCrearDto(int ProductoId, decimal Cantidad);

public record VentaCrearDto(MedioPago MedioPago, List<VentaItemCrearDto> Items);

/// <summary>PrecioUnitario y CostoUnitario son los vigentes al momento de la venta.</summary>
public record VentaItemDto(
    int ProductoId,
    string Producto,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal CostoUnitario,
    decimal Subtotal,
    decimal Ganancia);

public record VentaDto(
    int Id,
    DateTime Fecha,
    decimal TotalVenta,
    MedioPago MedioPago,
    decimal CostoTotal,
    decimal Ganancia,
    List<VentaItemDto> Items);
