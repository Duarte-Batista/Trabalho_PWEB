using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoLoja.Entities
{
	public class ItemEncomenda
	{
		public int Id { get; set; }
		public int Quantidade { get; set; }
		
		[Column(TypeName = "decimal(10,2)")]
		public decimal PrecoUnitario { get; set; }
		
		public int EncomendaId { get; set; }
		public Encomenda? Encomenda { get; set; }

		public int ProdutoId { get; set; }
		public Produto? Produto { get; set; }
	}
}
