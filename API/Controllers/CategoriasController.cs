using GestaoLoja.Data;
using GestaoLoja.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

		// GET: api/Categorias (Público)
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
		{
			return await _context.Categorias.ToListAsync();
		}

		// GET: api/Categorias/5 (Público)
		[HttpGet("{id}")]
		public async Task<ActionResult<Categoria>> GetCategoria(int id)
		{
			var categoria = await _context.Categorias.FindAsync(id);
			if (categoria == null) return NotFound();
			return categoria;
		}

		// POST: api/Categorias (Só Admin)
		[HttpPost]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
		{
			_context.Categorias.Add(categoria);
			await _context.SaveChangesAsync();
			return CreatedAtAction("GetCategoria", new { id = categoria.Id }, categoria);
		}

		// PUT: api/Categorias/5 (Só Admin)
		[HttpPut("{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
		{
			if (id != categoria.Id) return BadRequest();

			_context.Entry(categoria).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!_context.Categorias.Any(e => e.Id == id)) return NotFound();
				else throw;
			}

			return NoContent();
		}

		// DELETE: api/Categorias/5 (Só Admin)
		[HttpDelete("{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> DeleteCategoria(int id)
		{
			var categoria = await _context.Categorias.FindAsync(id);
			if (categoria == null) return NotFound();

			// Proteção: Não apagar se tiver produtos
			var temProdutos = await _context.Produtos.AnyAsync(p => p.CategoriaId == id);
			if (temProdutos)
				return BadRequest("Não é possível apagar esta categoria pois tem produtos associados.");

			_context.Categorias.Remove(categoria);
			await _context.SaveChangesAsync();

			return NoContent();
		}
	}
}