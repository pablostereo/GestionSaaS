using System.ComponentModel.DataAnnotations.Schema;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Entidades;

public class Venta : IEntidadCliente
{
    public int Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalVenta { get; set; }

    public MedioPago MedioPago { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
