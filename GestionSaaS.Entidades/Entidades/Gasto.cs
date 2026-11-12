using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Entidades;

/// <summary>
/// Egreso operativo (luz, alquiler, impuestos…). Solo registra la salida de dinero:
/// no toca productos ni stock.
/// </summary>
public class Gasto : IEntidadCliente
{
    public int Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTime Fecha { get; set; }

    [Required, MaxLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    public CategoriaGasto Categoria { get; set; }

    public MedioPago MedioPago { get; set; }
}
