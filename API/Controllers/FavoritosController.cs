using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GestaoLoja.Data;
using GestaoLoja.Entities;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] 
	public class FavoritosController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public FavoritosController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Favoritos (Ver a minha lista)
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Produto>>> GetMeusFavoritos()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Vai buscar os favoritos e INCLUI os dados do produto
			var produtosFavoritos = await _context.Favoritos
				.Where(f => f.ClienteId == userId)
				.Include(f => f.Produto) // Carrega o produto
					.ThenInclude(p => p.Categoria)
				.Select(f => f.Produto!) // Seleciona apenas o objeto Produto para enviar
				.ToListAsync();

			return produtosFavoritos;
		}

		// GET: api/Favoritos/ids (Saber quais os IDs que gosto)
		[HttpGet("ids")]
		public async Task<ActionResult<IEnumerable<int>>> GetIdsFavoritos()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			return await _context.Favoritos
				.Where(f => f.ClienteId == userId)
				.Select(f => f.ProdutoId)
				.ToListAsync();
		}

		// POST: api/Favoritos/toggle/5 (Adicionar ou Remover)
		[HttpPost("toggle/{produtoId}")]
		public async Task<IActionResult> ToggleFavorito(int produtoId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Verificar se o produto existe
			var produto = await _context.Produtos.FindAsync(produtoId);
			if (produto == null) return NotFound("Produto não existe.");

			// Verificar se já é favorito
			var favoritoExistente = await _context.Favoritos
				.FirstOrDefaultAsync(f => f.ClienteId == userId && f.ProdutoId == produtoId);

			if (favoritoExistente != null)
			{
				// JÁ EXISTE -> REMOVER (Desgostar)
				_context.Favoritos.Remove(favoritoExistente);
				await _context.SaveChangesAsync();
				return Ok(new { message = "Removido dos favoritos", isFavorito = false });
			}
			else
			{
				// NÃO EXISTE -> ADICIONAR (Gostar)
				var novoFavorito = new Favorito
				{
					ClienteId = userId!,
					ProdutoId = produtoId
				};
				_context.Favoritos.Add(novoFavorito);
				await _context.SaveChangesAsync();
				return Ok(new { message = "Adicionado aos favoritos", isFavorito = true });
			}
		}
	}
}