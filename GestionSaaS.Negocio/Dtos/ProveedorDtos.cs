namespace GestionSaaS.Negocio.Dtos;

public record ProveedorGuardarDto(string Nombre, string? Cuit, string? Telefono, string? Email);

public record ProveedorDto(
    int Id,
    string Nombre,
    string? Cuit,
    string? Telefono,
    string? Email,
    bool Activo,
    int CantidadCompras,
    decimal TotalComprado,
    DateTime? UltimaCompra);
