using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoLoja.Data;      // Confirma se este é o namespace do teu DbContext
using GestaoLoja.Entities;  // Confirma se este é o namespace dos teus Produtos

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProdutosController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		// Injeção de dependência: A API recebe a ligação à BD aqui
		public ProdutosController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Produtos
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
		{
			// Vai buscar à BD apenas os produtos que:
			// 1. Têm estado "Ativo" (Aprovados pelo Admin)
			// 2. Estão marcados como "DisponívelParaVenda" (Não são só coleção)
			// 3. Inclui a Categoria para sabermos o nome dela
			return await _context.Produtos
				.Include(p => p.Categoria)
				.Where(p => p.Estado == EstadoProduto.Ativo && p.DisponivelParaVenda == true)
				.ToListAsync();
		}

		// GET: api/Produtos/5 (Para ver detalhes de um produto específico)
		[HttpGet("{id}")]
		public async Task<ActionResult<Produto>> GetProduto(int id)
		{
			var produto = await _context.Produtos
				.Include(p => p.Categoria)
				.Include(p => p.Fornecedor) // Inclui o fornecedor nos detalhes
				.FirstOrDefaultAsync(p => p.Id == id);

			if (produto == null)
			{
				return NotFound();
			}

			return produto;
		}
	}
}