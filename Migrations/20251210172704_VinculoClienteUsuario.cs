using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSales.Migrations
{
    /// <inheritdoc />
    public partial class VinculoClienteUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Clientes");
        }
    }
}
