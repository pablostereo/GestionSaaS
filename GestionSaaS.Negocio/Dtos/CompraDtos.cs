namespace GestionSaaS.Negocio.Dtos;

public record CompraItemCrearDto(int ProductoId, decimal Cantidad, decimal CostoUnitario);

/// <summary>Reposición de mercadería: proveedor + productos. El total se calcula solo.</summary>
public record CompraCrearDto(
    int ProveedorId,
    List<CompraItemCrearDto> Items,
    string? NumeroComprobante,
    string? Observaciones,
    DateTime? Fecha);

public record CompraItemDto(int ProductoId, string Producto, decimal Cantidad, decimal CostoUnitario, decimal Subtotal);

public record CompraDto(
    int Id,
    DateTime Fecha,
    int ProveedorId,
    string Proveedor,
    string? NumeroComprobante,
    string? Observaciones,
    decimal MontoTotal,
    List<CompraItemDto> Items);
