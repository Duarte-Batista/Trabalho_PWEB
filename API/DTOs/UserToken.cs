namespace API.DTOs
{
	public class UserToken
	{
		public string Token { get; set; } = string.Empty;
		public DateTime Expiration { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
	}
}