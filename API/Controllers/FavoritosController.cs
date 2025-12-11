using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GestaoLoja.Data;
using GestaoLoja.Entities;

namespace MyCOLL.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // Obrigatório estar logado
	public class FavoritosController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public FavoritosController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Favoritos?page=1&pageSize=10 (COM PAGINAÇÃO)
		// Antes devolvia tudo. Agora devolve apenas 10 de cada vez para a App ser rápida.
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Produto>>> GetMeusFavoritos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var query = _context.Favoritos
				.Where(f => f.ClienteId == userId)
				.OrderByDescending(f => f.DataAdicionado) // Os mais recentes primeiro
				.Include(f => f.Produto)
					.ThenInclude(p => p.Categoria)
				.Select(f => f.Produto!); // Seleciona o objeto Produto

			// Aplica a paginação
			var produtos = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return produtos;
		}

		// GET: api/Favoritos/ids (Para pintar os corações na lista)
		[HttpGet("ids")]
		public async Task<ActionResult<IEnumerable<int>>> GetIdsFavoritos()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			return await _context.Favoritos
				.Where(f => f.ClienteId == userId)
				.Select(f => f.ProdutoId)
				.ToListAsync();
		}

		// POST: api/Favoritos/toggle/5 (O tal botão inteligente: Adiciona/Remove)
		[HttpPost("toggle/{produtoId}")]
		public async Task<IActionResult> ToggleFavorito(int produtoId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Verificar se produto existe
			var produto = await _context.Produtos.FindAsync(produtoId);
			if (produto == null) return NotFound("Produto não encontrado.");

			var favorito = await _context.Favoritos
				.FirstOrDefaultAsync(f => f.ClienteId == userId && f.ProdutoId == produtoId);

			if (favorito != null)
			{
				// Já existe -> Remove
				_context.Favoritos.Remove(favorito);
				await _context.SaveChangesAsync();
				return Ok(new { message = "Removido", isFavorito = false });
			}
			else
			{
				// Não existe -> Cria
				var novo = new Favorito { ClienteId = userId!, ProdutoId = produtoId };
				_context.Favoritos.Add(novo);
				await _context.SaveChangesAsync();
				return Ok(new { message = "Adicionado", isFavorito = true });
			}
		}

		// DELETE: api/Favoritos/5 (NOVO - Remoção Explícita)
		// Útil para quando fazes "Swipe to Delete" na App
		[HttpDelete("{produtoId}")]
		public async Task<IActionResult> RemoveFavorito(int produtoId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var favorito = await _context.Favoritos
				.FirstOrDefaultAsync(f => f.ClienteId == userId && f.ProdutoId == produtoId);

			if (favorito == null)
			{
				// Se já não existia, tudo bem, o objetivo era não ter lá nada.
				return NotFound("Este produto não estava nos favoritos.");
			}

			_context.Favoritos.Remove(favorito);
			await _context.SaveChangesAsync();

			return NoContent(); // 204 No Content (Sucesso padrão para Delete)
		}

		// DELETE: api/Favoritos/todos (NOVO - Limpar Lista)
		[HttpDelete("todos")]
		public async Task<IActionResult> ClearFavoritos()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Vai buscar todos os favoritos deste user
			var meusFavoritos = await _context.Favoritos
				.Where(f => f.ClienteId == userId)
				.ToListAsync();

			if (!meusFavoritos.Any()) return Ok("A lista já estava vazia.");

			_context.Favoritos.RemoveRange(meusFavoritos);
			await _context.SaveChangesAsync();

			return Ok(new { message = $"{meusFavoritos.Count} favoritos removidos." });
		}
	}
}