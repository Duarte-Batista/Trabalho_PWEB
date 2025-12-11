using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GestaoLoja.Data;
using GestaoLoja.Entities;
using API.DTOs;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Obrigatório estar logado para comprar
	public class EncomendasController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public EncomendasController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Encomendas (Histórico do Cliente)
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ResumoEncomendaDto>>> GetMinhasEncomendas()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Vai buscar as encomendas DESTE utilizador
			var encomendas = await _context.Encomendas
				.Where(e => e.ClienteId == userId)
				.OrderByDescending(e => e.Data)
				.Select(e => new ResumoEncomendaDto
				{
					Id = e.Id,
					Data = e.Data,
					Total = e.ValorTotal,
					Estado = e.Estado
				})
				.ToListAsync();

			return encomendas;
		}

		// GET: api/Encomendas/5 (Detalhes de uma Encomenda)
		[HttpGet("{id}")]
		public async Task<ActionResult<Encomenda>> GetDetalhesEncomenda(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var encomenda = await _context.Encomendas
				.Include(e => e.Itens)
				.ThenInclude(i => i.Produto) // Inclui dados do produto (Nome, Imagem)
				.FirstOrDefaultAsync(e => e.Id == id && e.ClienteId == userId);

			if (encomenda == null) return NotFound();

			return encomenda;
		}

		// POST: api/Encomendas (CRIAR NOVA COMPRA / CHECKOUT)
		[HttpPost]
		public async Task<ActionResult<ResumoEncomendaDto>> CriarEncomenda([FromBody] CriarEncomendaDto carrinho)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (carrinho.Itens == null || !carrinho.Itens.Any()) return BadRequest("Carrinho vazio");

			// Criar a Encomenda (Cabeçalho)
			var novaEncomenda = new Encomenda
			{
				ClienteId = userId,
				Data = DateTime.Now,
				Estado = "Pendente",
				ValorTotal = 0,
				Itens = new List<ItemEncomenda>()
			};

			// Processar Itens
			foreach (var itemDto in carrinho.Itens)
			{
				var produto = await _context.Produtos.FindAsync(itemDto.ProdutoId);

				if (produto == null) return BadRequest($"Produto {itemDto.ProdutoId} não existe");
				if (!produto.DisponivelParaVenda) return BadRequest($"Produto {produto.Nome} não está à venda");
				if (produto.Stock < itemDto.Quantidade) return BadRequest($"Stock insuficiente para {produto.Nome}");

				// Adicionar Item
				var novoItem = new ItemEncomenda
				{
					ProdutoId = produto.Id,
					Quantidade = itemDto.Quantidade,
					PrecoUnitario = produto.PrecoFinal // Guarda o preço ATUAL (Snapshot)
				};

				novaEncomenda.Itens.Add(novoItem);
				novaEncomenda.ValorTotal += novoItem.PrecoUnitario * novoItem.Quantidade;
			}

			_context.Encomendas.Add(novaEncomenda);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetDetalhesEncomenda), new { id = novaEncomenda.Id }, new ResumoEncomendaDto
			{
				Id = novaEncomenda.Id,
				Data = novaEncomenda.Data,
				Total = novaEncomenda.ValorTotal,
				Estado = novaEncomenda.Estado
			});
		}

		// POST: api/Encomendas/5/pagar (SIMULAR PAGAMENTO)
		[HttpPost("{id}/pagar")]
		public async Task<IActionResult> SimularPagamento(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var encomenda = await _context.Encomendas
				.Include(e => e.Itens)
				.FirstOrDefaultAsync(e => e.Id == id && e.ClienteId == userId);

			if (encomenda == null) return NotFound();

			if (encomenda.Estado != "Pendente")
				return BadRequest("Esta encomenda já foi processada");

			// Validação extra: A encomenda tem itens?
			if (encomenda.Itens == null || !encomenda.Itens.Any())
			{
				return BadRequest("Erro Crítico: A encomenda não tem itens registados na base de dados");
			}

			foreach (var item in encomenda.Itens)
			{
				var produto = await _context.Produtos.FindAsync(item.ProdutoId);
				if (produto != null)
				{
					// Verifica se ainda há stock antes de cobrar
					if (produto.Stock < item.Quantidade)
					{
						return BadRequest($"Erro: Stock insuficiente para o produto {produto.Nome}");
					}

					// Abate o stock
					produto.Stock -= item.Quantidade;

					_context.Entry(produto).State = EntityState.Modified;
				}
			}

			// Simulação de Sucesso
			encomenda.Estado = "Pago";
			_context.Entry(encomenda).State = EntityState.Modified;

			await _context.SaveChangesAsync();

			return Ok(new { message = "Pagamento efetuado e stock atualizado!", novoEstado = "Pago" });
		}

		// PUT: api/Encomendas/5 (Atualizar Estado Manualmente - Admin)
		[HttpPut("{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> UpdateEstadoEncomenda(int id, [FromBody] string novoEstado)
		{
			var encomenda = await _context.Encomendas.FindAsync(id);
			if (encomenda == null) return NotFound();

			encomenda.Estado = novoEstado;
			await _context.SaveChangesAsync();

			return Ok(new { message = $"Estado alterado para {novoEstado}" });
		}

		// DELETE: api/Encomendas/5 (Apagar Encomenda - Admin)
		[HttpDelete("{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> DeleteEncomenda(int id)
		{
			var encomenda = await _context.Encomendas.FindAsync(id);
			if (encomenda == null) return NotFound();

			_context.Encomendas.Remove(encomenda);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}