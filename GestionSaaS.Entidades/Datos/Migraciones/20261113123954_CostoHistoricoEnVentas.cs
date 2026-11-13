using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionSaaS.Entidades.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class CostoHistoricoEnVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostoUnitario",
                table: "DetallesVenta",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Ventas anteriores a este cambio: no se conoce su costo histórico.
            // Se completa con el costo actual del producto como mejor estimación.
            migrationBuilder.Sql(
                """
                UPDATE dv SET dv.CostoUnitario = p.CostoActual
                FROM DetallesVenta dv
                INNER JOIN Productos p ON p.Id = dv.ProductoId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoUnitario",
                table: "DetallesVenta");
        }
    }
}
