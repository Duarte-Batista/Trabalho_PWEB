using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
	public class ChangePasswordDTO
	{
		[Required]
		public string CurrentPassword { get; set; } = string.Empty;

		[Required]
		[MinLength(6)]
		public string NewPassword { get; set; } = string.Empty;
	}
}
