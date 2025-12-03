using GestaoLoja.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoLoja.Entities
{
	public class Encomenda
	{
		public int Id { get; set; }
		public DateTime Data { get; set; } = DateTime.Now;
		[Column(TypeName = "decimal(10,2)")]
		public decimal ValorTotal { get; set; }

		public string Estado { get; set; } = "Pendente";
		public string? ClienteId { get; set; }

		[ForeignKey("ClienteId")]
		public ApplicationUser? Cliente { get; set; }

		public List<ItemEncomenda> Itens { get; set; } = new();
	}
}
