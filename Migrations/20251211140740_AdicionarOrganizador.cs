using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSales.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarOrganizador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizadorId",
                table: "Eventos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizadorId",
                table: "Eventos");
        }
    }
}
