namespace API.DTOs
{
	public class UserInfoResult
	{
		public string Email { get; set; } = string.Empty;
		public string Nif { get; set; } = string.Empty;
		public string Morada { get; set; } = string.Empty;
		public string PhoneNumber { get; set; } = string.Empty;
		public bool IsEmailConfirmed { get; set; }
	}
}
