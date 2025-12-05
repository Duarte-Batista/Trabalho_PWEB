using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoLoja.Data;
using GestaoLoja.Entities;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CategoriasController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public CategoriasController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Categorias
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
		{
			// Retorna a árvore completa (Categorias -> Subcategorias)
			return await _context.Categorias
				.Where(c => c.CategoriaPaiId == null) // Apenas as Raiz
				.Include(c => c.SubCategorias) // Inclui filhas
				.ToListAsync();
		}

		// GET: api/Categorias/Flat
		// Retorna todas numa lista simples (útil para dropdowns)
		[HttpGet("Flat")]
		public async Task<ActionResult<IEnumerable<Categoria>>> GetTodasCategorias()
		{
			return await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
		}

		// GET: api/Categorias/5
		[HttpGet("{id}")]
		public async Task<ActionResult<Categoria>> GetCategoria(int id)
		{
			var categoria = await _context.Categorias
				.Include(c => c.SubCategorias)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (categoria == null) return NotFound();

			return categoria;
		}
	}
}