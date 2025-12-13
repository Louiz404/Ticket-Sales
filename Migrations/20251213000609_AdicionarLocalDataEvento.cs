using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSales.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLocalDataEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataEvento",
                table: "Eventos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Eventos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Local",
                table: "Eventos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataEvento",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "Local",
                table: "Eventos");
        }
    }
}
