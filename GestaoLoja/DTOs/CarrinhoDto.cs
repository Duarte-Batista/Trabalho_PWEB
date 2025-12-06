namespace GestaoLoja.DTOs
{
	// O que o cliente envia quando clica em "Finalizar Compra"
	public class CriarEncomendaDto
	{
		public List<ItemCarrinhoDto> Itens { get; set; } = new();
	}

	public class ItemCarrinhoDto
	{
		public int ProdutoId { get; set; }
		public int Quantidade { get; set; }
	}

	// O que devolvemos para mostrar o recibo simples
	public class ResumoEncomendaDto
	{
		public int Id { get; set; }
		public DateTime Data { get; set; }
		public decimal Total { get; set; }
		public string Estado { get; set; } = string.Empty;
	}
}