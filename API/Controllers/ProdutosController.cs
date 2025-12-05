using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoLoja.Data;
using GestaoLoja.Entities;
using System.Security.Claims; // Necessário para ler o ID do utilizador (futuro JWT)

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProdutosController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _environment; // Para guardar imagens

		public ProdutosController(ApplicationDbContext context, IWebHostEnvironment environment)
		{
			_context = context;
			_environment = environment;
		}

		// 1. GET PÚBLICO (Com Filtros)
		// Exemplo: GET /api/produtos?termo=moeda&categoriaId=2
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos([FromQuery] string? termo, [FromQuery] int? categoriaId)
		{
			var query = _context.Produtos
				.Include(p => p.Categoria)
				.Where(p => p.Estado == EstadoProduto.Ativo && p.DisponivelParaVenda == true)
				.AsQueryable();

			if (!string.IsNullOrEmpty(termo))
			{
				query = query.Where(p => p.Nome.Contains(termo) || p.Descricao.Contains(termo));
			}

			if (categoriaId.HasValue && categoriaId > 0)
			{
				query = query.Where(p => p.CategoriaId == categoriaId);
			}

			return await query.ToListAsync();
		}

		// 2. GET DETALHES
		[HttpGet("{id}")]
		public async Task<ActionResult<Produto>> GetProduto(int id)
		{
			var produto = await _context.Produtos
				.Include(p => p.Categoria)
				.Include(p => p.Fornecedor)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (produto == null) return NotFound();

			return produto;
		}

		// ==========================================================
		// ÁREA DO FORNECEDOR (Requer Autenticação no futuro)
		// ==========================================================

		// 3. GET MEUS PRODUTOS (Para o Fornecedor ver a sua lista)
		[HttpGet("meus-produtos")]
		public async Task<ActionResult<IEnumerable<Produto>>> GetMeusProdutos()
		{
			// Nota: Quando tivermos JWT, trocamos "user-temp" pelo User.FindFirstValue(ClaimTypes.NameIdentifier)
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId)) return Unauthorized("Utilizador não identificado.");

			return await _context.Produtos
				.Include(p => p.Categoria)
				.Where(p => p.FornecedorId == userId)
				.ToListAsync();
		}

		// 4. CRIAR PRODUTO (POST)
		[HttpPost]
		public async Task<ActionResult<Produto>> PostProduto([FromForm] Produto produto)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) return Unauthorized();

			// Regras de negócio
			produto.FornecedorId = userId;
			produto.Estado = EstadoProduto.Pendente; // Começa sempre Pendente!

			// Tratamento de Imagem (Se vier no form)
			if (produto.ImageFile != null)
			{
				produto.UrlImagem = await GuardarImagem(produto.ImageFile);
			}

			_context.Produtos.Add(produto);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetProduto", new { id = produto.Id }, produto);
		}

		// 5. EDITAR PRODUTO (PUT)
		[HttpPut("{id}")]
		public async Task<IActionResult> PutProduto(int id, [FromForm] Produto produtoAtualizado)
		{
			if (id != produtoAtualizado.Id) return BadRequest();

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var produtoOriginal = await _context.Produtos.FindAsync(id);

			// Segurança: Só o dono pode editar
			if (produtoOriginal == null) return NotFound();
			if (produtoOriginal.FornecedorId != userId) return Forbid(); // 403 Proibido

			// Atualizar campos permitidos
			produtoOriginal.Nome = produtoAtualizado.Nome;
			produtoOriginal.Descricao = produtoAtualizado.Descricao;
			produtoOriginal.PrecoBase = produtoAtualizado.PrecoBase;
			produtoOriginal.Stock = produtoAtualizado.Stock;
			produtoOriginal.CategoriaId = produtoAtualizado.CategoriaId;
			produtoOriginal.DisponivelParaVenda = produtoAtualizado.DisponivelParaVenda;

			// Se editou, volta a Pendente para aprovação? (Depende da regra, geralmente sim)
			produtoOriginal.Estado = EstadoProduto.Pendente;

			if (produtoAtualizado.ImageFile != null)
			{
				produtoOriginal.UrlImagem = await GuardarImagem(produtoAtualizado.ImageFile);
			}

			await _context.SaveChangesAsync();
			return NoContent();
		}

		// 6. APAGAR PRODUTO (DELETE)
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteProduto(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var produto = await _context.Produtos.FindAsync(id);

			if (produto == null) return NotFound();
			if (produto.FornecedorId != userId) return Forbid();

			// Verificar se tem vendas (Regra de Ouro)
			bool temVendas = await _context.ItensEncomenda.AnyAsync(i => i.ProdutoId == id);
			if (temVendas)
			{
				// Soft delete (Inativo) em vez de apagar
				produto.Estado = EstadoProduto.Inativo;
				await _context.SaveChangesAsync();
				return Ok(new { message = "Produto inativado pois já tem vendas." });
			}

			_context.Produtos.Remove(produto);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		// Função Auxiliar para Upload
		private async Task<string> GuardarImagem(IFormFile ficheiro)
		{
			var nomeUnico = Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName);
			var pastaUploads = Path.Combine(_environment.WebRootPath, "img", "produtos");

			if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

			var caminhoFicheiro = Path.Combine(pastaUploads, nomeUnico);

			using (var stream = new FileStream(caminhoFicheiro, FileMode.Create))
			{
				await ficheiro.CopyToAsync(stream);
			}

			return $"/img/produtos/{nomeUnico}";
		}
	}
}