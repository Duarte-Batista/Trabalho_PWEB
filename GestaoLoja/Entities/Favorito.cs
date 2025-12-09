using GestaoLoja.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestaoLoja.Entities
{
	public class Favorito
	{
		public int Id { get; set; }

		[Required]
		public string ClienteId { get; set; } = string.Empty;
		[ForeignKey("ClienteId")]
		public ApplicationUser? Cliente { get; set; }

		[Required]
		public int ProdutoId { get; set; }
		[ForeignKey("ProdutoId")]
		public Produto? Produto { get; set; }

		public DateTime DataAdicionado { get; set; } = DateTime.Now;
	}
}
