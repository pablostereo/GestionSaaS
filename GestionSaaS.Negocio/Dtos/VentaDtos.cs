using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Negocio.Dtos;

public record VentaItemCrearDto(int ProductoId, decimal Cantidad);

public record VentaCrearDto(MedioPago MedioPago, List<VentaItemCrearDto> Items);

public record VentaItemDto(int ProductoId, string Producto, decimal Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record VentaDto(int Id, DateTime Fecha, decimal TotalVenta, MedioPago MedioPago, List<VentaItemDto> Items);
