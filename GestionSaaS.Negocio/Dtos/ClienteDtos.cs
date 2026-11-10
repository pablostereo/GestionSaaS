namespace GestionSaaS.Negocio.Dtos;

/// <summary>Alta de cliente: crea el comercio y su usuario de acceso (mismo email).</summary>
public record ClienteCrearDto(string NombreComercio, string Email, string Contrasena);

public record ClienteActualizarDto(string NombreComercio, string Email);

public record RestablecerContrasenaDto(string Contrasena);

public record ClienteDto(
    Guid Id,
    string NombreComercio,
    string Email,
    DateTime FechaAlta,
    bool Activo,
    DateTime? UltimoIngreso,
    int CantidadProductos,
    int CantidadVentas,
    decimal TotalVendido);
