using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace GestaoLoja.Entities
{
	public class Categoria
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "O nome da categoria é obrigatório")]
		public string Nome { get; set; } = string.Empty;
		public int? Ordem {  get; set; }
		public string? UrlImagem { get; set; }
		public byte[]? Imagem { get; set; }
		
		[NotMapped]
		public IFormFile? ImageFile { get; set; }

		//para sub categorias (ex: moedas -> portugal -> ouro)
		public int? CategoriaPaiId { get; set; }

		[ForeignKey("CategoriaPaiId")]
		public Categoria? CategoriaPai {  get; set; }

		public ICollection<Categoria>? SubCategorias { get; set; }
		public ICollection<Produto>? Produtos { get; set; } 
	}
}
