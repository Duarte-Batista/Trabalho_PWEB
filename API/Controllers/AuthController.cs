using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestaoLoja.DTOs;
using GestaoLoja.Data;

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

		// POST: api/auth/register
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

			// Adicionar Role
			await _userManager.AddToRoleAsync(user, "Cliente");

			// Gerar Token
			return await GerarToken(user);
		}

		// POST: api/auth/login
		[HttpPost("login")]
		public async Task<ActionResult<UserToken>> Login([FromBody] LoginDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null) return BadRequest("Login inválido.");

			if (user.EstadoConta != "Ativo")
			{
				return Unauthorized("Conta pendente ou suspensa.");
			}

			var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
			if (!result.Succeeded) return BadRequest("Login inválido.");

			return await GerarToken(user);
		}

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

			// Lê a chave do appsettings.json da API
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "chave_super_secreta_fallback_12345"));
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