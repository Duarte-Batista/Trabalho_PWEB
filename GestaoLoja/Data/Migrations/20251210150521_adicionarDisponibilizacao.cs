using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Migrations
{
    /// <inheritdoc />
    public partial class adicionarDisponibilizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModoEntrega",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MoradaEnvio",
                table: "Encomendas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPortes",
                table: "Encomendas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModoEntrega",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "MoradaEnvio",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "ValorPortes",
                table: "Encomendas");
        }
    }
}
