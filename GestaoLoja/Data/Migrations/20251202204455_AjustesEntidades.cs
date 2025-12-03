using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Migrations
{
    /// <inheritdoc />
    public partial class AjustesEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Imagem",
                table: "Produtos",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlImagem",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Imagem",
                table: "Categorias",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "Categorias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlImagem",
                table: "Categorias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagem",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "UrlImagem",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Imagem",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "UrlImagem",
                table: "Categorias");
        }
    }
}
