using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Negocio.Dtos;

public record GastoGuardarDto(
    string Descripcion,
    decimal Monto,
    DateTime Fecha,
    CategoriaGasto Categoria,
    MedioPago MedioPago);

public record GastoDto(
    int Id,
    DateTime Fecha,
    string Descripcion,
    decimal Monto,
    CategoriaGasto Categoria,
    MedioPago MedioPago);
