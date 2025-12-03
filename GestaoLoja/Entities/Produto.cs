using GestaoLoja.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoLoja.Entities
{
	public enum EstadoProduto { Pendente, Ativo, Inativo, Vendido }

	public class Produto
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "O nome é obrigatório")]
		public string Nome { get; set; } = string.Empty;
		public string Descricao { get; set; } = string.Empty;

		[Column(TypeName = "decimal(10, 2)")]
		[Display(Name = "Preço Base")]
		public decimal PrecoBase { get; set; }

		[Display(Name = "Margem de Lucro (%)")]
		public double MargemLucro { get; set; } = 0.0;

		[NotMapped]
		public decimal PrecoFinal => PrecoBase + (PrecoBase * (decimal)MargemLucro);
		public string ImageUrl { get; set; } = string.Empty;
		public EstadoProduto Estado { get; set; } = EstadoProduto.Pendente;

		public string? UrlImagem { get; set; }

		public byte[]? Imagem { get; set; } 

		[NotMapped]
		public IFormFile? ImageFile { get; set; }

		public int Stock { get; set; } = 1;
		public bool DisponivelParaVenda { get; set; } = true;

		public int CategoriaId { get; set; }
		public Categoria? Categoria { get; set; }

		public string? FornecedorId { get; set; }
		public ApplicationUser? Fornecedor { get; set; }
	}
}
