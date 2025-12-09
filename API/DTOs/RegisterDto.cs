using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
	public class RegisterDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;

		[Compare("Password", ErrorMessage = "As passwords não coincidem")]
		public string ConfirmPassword { get; set; } = string.Empty;

		public string Nif { get; set; } = string.Empty;
		public string Morada { get; set; } = string.Empty;
	}
}