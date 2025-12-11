using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestaoLoja.Data;
using GestaoLoja.Entities;
using API.DTOs;    

namespace MyCOLL.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IConfiguration _configuration;

		public AuthController(UserManager<ApplicationUser> userManager,
							  SignInManager<ApplicationUser> signInManager,
							  IConfiguration configuration)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
		}

		// POST: /identity/register
		[HttpPost("register")]
		public async Task<ActionResult<UserToken>> Register([FromBody] RegisterDto model)
		{
			var user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				Nif = model.Nif,     
				Morada = model.Morada, 
				TipoUtilizador = "Cliente",
				EstadoConta = "Pendente"
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded) return BadRequest(result.Errors);

			await _userManager.AddToRoleAsync(user, "Cliente");

			// Nota: Retorna token, mas se estiver pendente o login falhará depois.
			// Opcional: Podes retornar apenas Ok("Registo efetuado. Aguarde aprovação.")
			return await GerarToken(user);
		}

		// POST: /identity/login
		[HttpPost("login")]
		public async Task<ActionResult<UserToken>> Login([FromBody] LoginDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null) return Unauthorized("Login inválido.");

			// A tua verificação de segurança extra
			if (user.EstadoConta != "Ativo")
			{
				return Unauthorized("A sua conta ainda está pendente de aprovação ou suspensa.");
			}

			var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
			if (!result.Succeeded) return Unauthorized("Login inválido.");

			return await GerarToken(user);
		}

		// POST: /identity/refresh (Simulação simples - Renovar Token)
		[HttpPost("refresh")]
		[Authorize]
		public async Task<ActionResult<UserToken>> Refresh()
		{
			// Obtém o utilizador atual pelo Token que enviou
			var user = await _userManager.GetUserAsync(User);
			if (user == null || user.EstadoConta != "Ativo") return Unauthorized();

			// Gera um token novo com validade renovada
			return await GerarToken(user);
		}

		// GET: /identity/manage/info (Ver dados do perfil)
		[HttpGet("manage/info")]
		[Authorize]
		public async Task<ActionResult<UserInfoResult>> GetInfo()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			return new UserInfoResult
			{
				Email = user.Email ?? "",
				Nif = user.Nif ?? "",
				Morada = user.Morada ?? "",
				PhoneNumber = user.PhoneNumber ?? "",
				IsEmailConfirmed = user.EmailConfirmed
			};
		}

		// POST: /identity/manage/info (Atualizar dados do perfil)
		[HttpPost("manage/info")]
		[Authorize]
		public async Task<ActionResult<UserInfoResult>> PostInfo([FromBody] UserInfoResult model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			// Atualiza os campos (exceto Email que é sensível)
			user.Nif = model.Nif;
			user.Morada = model.Morada;
			user.PhoneNumber = model.PhoneNumber;

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded) return BadRequest(result.Errors);

			return Ok(model);
		}

		// POST: /identity/manage/password (Mudar password)
		[HttpPost("manage/password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

			if (!result.Succeeded) return BadRequest(result.Errors);

			return Ok(new { message = "Password alterada com sucesso." });
		}

		// POST: /identity/forgotPassword (Simulação)
		[HttpPost("forgotPassword")]
		public IActionResult ForgotPassword([FromBody] object model)
		{
			// Num sistema real, aqui enviava o email.
			// Para este projeto, só simula que o pedido foi aceite
			return Ok(new { message = "Se o email existir, enviámos um link de recuperação." });
		}

		// PUT: /identity/users/{id}/estado
		[HttpPut("users/{id}/estado")]
		[Authorize(Roles = "Administrador")]
		public async Task<IActionResult> ChangeUserState(string id, [FromBody] string novoEstado)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user == null) return NotFound("Utilizador não encontrado.");

			// Validação simples para evitar estados inventados
			var estadosValidos = new[] { "Ativo", "Pendente", "Suspenso" };
			if (!estadosValidos.Contains(novoEstado))
			{
				return BadRequest("Estado inválido. Use: Ativo, Pendente ou Suspenso.");
			}

			user.EstadoConta = novoEstado;

			// O UpdateAsync grava na BD
			var result = await _userManager.UpdateAsync(user);

			if (!result.Succeeded) return BadRequest(result.Errors);

			return Ok(new { message = $"Estado do utilizador {user.Email} alterado para {novoEstado}." });
		}

		// --- MÉTODOS AUXILIARES ---
		private async Task<UserToken> GerarToken(ApplicationUser user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, user.UserName ?? ""),
				new Claim(ClaimTypes.Email, user.Email ?? ""),
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim("Estado", user.EstadoConta)
			};

			var roles = await _userManager.GetRolesAsync(user);
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "chave_super_secreta_fallback"));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expiration = DateTime.Now.AddDays(1);

			var token = new JwtSecurityToken(
				issuer: "MyCOLL.API",
				audience: "MyCOLL.Frontend",
				claims: claims,
				expires: expiration,
				signingCredentials: creds
			);

			return new UserToken
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				Expiration = expiration,
				UserName = user.UserName ?? "",
				Role = roles.FirstOrDefault() ?? "Cliente"
			};
		}
	}
}