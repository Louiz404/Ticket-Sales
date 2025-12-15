using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSales.Migrations
{
    /// <inheritdoc />
    public partial class Coordenadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Eventos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Eventos",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Eventos");
        }
    }
}
