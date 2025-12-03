using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisponivelParaVenda",
                table: "Produtos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisponivelParaVenda",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Produtos");
        }
    }
}
