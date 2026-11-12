namespace GestionSaaS.Entidades.Enums;

/// <summary>Rubro de un gasto operativo (no incluye mercadería: eso es una Compra).</summary>
public enum CategoriaGasto
{
    Alquiler = 1,
    Luz = 2,
    Gas = 3,
    Agua = 4,
    Internet = 5,
    Telefonia = 6,
    Impuestos = 7,
    Sueldos = 8,
    Servicios = 9,
    Mantenimiento = 10,
    Otros = 99
}
